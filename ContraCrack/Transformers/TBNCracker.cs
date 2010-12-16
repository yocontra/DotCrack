using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;
using System.Windows.Forms;

namespace ContraCrack.Transformers
{
    class TBNCracker : Transformer
    {
        //HEY NIGGA
        //Notes: We need to add something to unclutter some of this fucking code
        //The dialog boxes to ask for user input are creating a shitload of clutter
        //So we need to put that somewhere else and call it from the transformers
        //ALSO DON'T FIND METHODS BY NAME! Use instruction patterns in the methods or properties
        //Scanning by name will completely get confuckered when obfuscation comes into play.

        LogHandler logger = new LogHandler("TBNCracker");
        string assemblyLocation;
        string newLocation;
        AssemblyDefinition assembly;
        bool flag = false;
        bool changed = false;

        public TBNCracker(string fileLoc)
        {
            logger.Log("TBNCracker Started!");
            assemblyLocation = fileLoc;
            newLocation = fileLoc.Replace(".exe", "-cracked.exe");
        }
        public void load()
        {
            logger.Log("Loading Assembly...");
            if (flag) return;
            try
            {
                assembly = AssemblyFactory.GetAssembly(assemblyLocation);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error loading assembly.");
                flag = true;
                return;
            }
            logger.Log("Removing Strongname Key...");
            assembly.Name.PublicKey = new byte[0];
            assembly.Name.PublicKeyToken = new byte[0];
            assembly.Name.Flags = AssemblyFlags.SideBySideCompatible;
        }
        private MethodDefinition appendMethod(MethodDefinition inputMethod, MethodDefinition appendMethod)
        {
            for (int x = 0; x < appendMethod.Body.Instructions.Count; x++)
            {
                inputMethod.Body.CilWorker.Append(appendMethod.Body.Instructions[x]);
            }
            return inputMethod;
        }
        public void transform()
        {
            if (flag) return;
            logger.Log("Starting Transformer...");
            foreach (TypeDefinition type in assembly.MainModule.Types)
            {
                if (type.Name != "<Module>")
                {
                    foreach (MethodDefinition method in type.Methods)
                    {
                        #region patch dialogresult return
                        if (method.HasBody && !method.IsAbstract
                            && !method.IsConstructor
                            && method.ReturnType.ReturnType.FullName.Contains("Void"))
                        {
                            //Gets the contents of the button click method (where it returns the dialogresult and closes)
                            //and append it to the end of the form constructor.
                            //still not working, see notes below.
                            //maybe move the code into form_load instead of the constructor
                            if (method.Body.Instructions.Count == 6
                                && method.Body.Instructions[0].OpCode == OpCodes.Ldarg_0
                                && method.Body.Instructions[1].OpCode == OpCodes.Ldc_I4_1
                                && method.Body.Instructions[2].OpCode == OpCodes.Callvirt
                                && method.Body.Instructions[3].OpCode == OpCodes.Ldarg_0
                                && method.Body.Instructions[4].OpCode == OpCodes.Callvirt
                                && method.Body.Instructions[5].OpCode == OpCodes.Ret)
                            {
                                DialogResult tz = MessageBox.Show("Method \"" + type.FullName + '.' + method.Name + "\" has met the search criteria.\r\n Crack it's Owner's constructor?", "Ay Papi!", MessageBoxButtons.YesNoCancel);
                                if (tz == DialogResult.Yes)
                                {
                                    logger.Log("Injecting method contents of  \"" + type.FullName + '.' + method.Name + "\" into  \"" + type.FullName + '.' + type.Constructors[0].Name + "\"");
                                    int count = method.Body.Instructions.Count;
                                    //Removes the callvirt to this.close() hopefully this will fix the issues
                                    //method.Body.CilWorker.Remove(method.Body.Instructions[count - 2]);
                                    //method.Body.CilWorker.Remove(method.Body.Instructions[count - 3]);
                                    //K nvm removing that fucks shit up neega but keeping it in causes an error.
                                    //wat do?
                                    MethodDefinition newdef = type.Constructors[0];
                                    newdef.Body.CilWorker.Remove(newdef.Body.Instructions[newdef.Body.Instructions.Count - 1]); //Nop out the ret so we can inject our return code
                                    type.Constructors[0] = appendMethod(newdef, method);
                                    type.Constructors[0].Body.Optimize();
                                    changed = true;
                                }
                                else if (tz == DialogResult.No)
                                {
                                    continue;
                                }
                                else
                                {
                                    return;
                                }
                            }
                        #endregion
                            #region patch initializecomponent
                            if (method.HasBody && !method.IsAbstract
                                && !method.IsConstructor
                                && method.IsPrivate
                                && method.ReturnType.ReturnType.FullName.Contains("Void")
                                && method.Parameters.Count == 0
                                //These next two lines should make sure it is an initializecomponent :)
                                && method.Body.Variables.Count >= 1
                                && method.Body.Variables[0].VariableType.FullName.ToString().Contains("ComponentResourceManager"))
                            {
                                DialogResult tz = MessageBox.Show("Method \"" + type.FullName + '.' + method.Name + "\" has met the search criteria. Patch Form Initializer?", "Ay Papi!", MessageBoxButtons.YesNoCancel);
                                if (tz == DialogResult.Yes)
                                {
                                    logger.Log("Modifying method \"" + type.FullName + '.' + method.Name + "\"");
                                    CilWorker worker;
                                    try
                                    {
                                        worker = method.Body.CilWorker;
                                    }
                                    catch (Exception e)
                                    {
                                        MessageBox.Show("Issue reading MSIL. Assembly is obfuscated or corrupt.");
                                        flag = true;
                                        return;
                                    }
                                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                                    {
                                        if (method.Body.Instructions.Count <= (i + 1)) break;
                                        //Enabled FUCKING EVERYTHING!!!
                                        if (method.Body.Instructions[i].OpCode == OpCodes.Ldc_I4_0
                                            && method.Body.Instructions[i + 1].OpCode == OpCodes.Callvirt
                                            && method.Body.Instructions[i + 1].Operand.ToString().Contains("set_Enabled"))
                                        {
                                            worker.Replace(method.Body.Instructions[i], worker.Create(OpCodes.Ldc_I4_1));
                                        }
                                    }
                                    method.Body.Optimize();
                                    changed = true;
                                }
                                else if (tz == DialogResult.No)
                                {
                                    continue;
                                }
                                else
                                {
                                    return;
                                }
                            }
                            #endregion
                            //Commented this out because it is nigga-buggy and not really needed. if you can figure it out
                            //props to you brahski and you should integrate it with the intialize patcher instead of
                            //being likes its own thing
                            /*
                            #region auth method
                            if (method.HasBody 
                                && method.Name.EndsWith("_Load"))
                                //Needs a better pattern than this lol
                            {
                                DialogResult tz = MessageBox.Show("Method \"" + type.FullName + '.' + method.Name + "\" has met the search criteria. Crack it?", "Ay Papi!", MessageBoxButtons.YesNoCancel);
                                if (tz == DialogResult.Yes)
                                {
                                    //Nigga you know what we should do is find the form with the auth, then set it to a variable
                                    //and scan through every method looking for calls and when we find a method that calls it
                                    //just nop dat shit or whuttt
                                    logger.Log("Modifying method \"" + type.FullName + '.' + method.Name + "\"");
                                    CilWorker worker;
                                    try
                                    {
                                        worker = method.Body.CilWorker;
                                    }
                                    catch (Exception e)
                                    {
                                        MessageBox.Show("Issue reading MSIL. Assembly is obfuscated or corrupt.");
                                        flag = true;
                                        return;
                                    }
                                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                                    {
                                        worker.Replace(method.Body.Instructions[i], worker.Create(OpCodes.Nop));
                                    }
                                    int count = method.Body.Instructions.Count;
                                    method.Body.ExceptionHandlers.Clear();
                                    method.Body.Variables.Clear();

                                    MethodInfo msg1 = typeof(System.Windows.Forms.Button).GetMethod("PerformClick");
                                    MethodReference msg2 = assembly.MainModule.Import(msg1);
                                    //I believe its ldargo.0 to load ProxyFetch.AuthForm or any form
                                    //Then you want to call the button and then call the method after it
                                    MethodInfo msg3 = typeof(System.Windows.Forms.Button).GetMethod("ProxyFetch.AuthForm::Button2");
                                    MethodReference msg4 = assembly.MainModule.Import(msg3);

                                    worker.Replace(method.Body.Instructions[count - 6], worker.Create(OpCodes.Ldarg_0));
                                    worker.Replace(method.Body.Instructions[count - 4], worker.Create(OpCodes.Nop));//ldfld, msg4
                                    worker.Replace(method.Body.Instructions[count - 2], worker.Create(OpCodes.Callvirt, msg2));
                                    worker.Replace(method.Body.Instructions[count - 1], worker.Create(OpCodes.Ret));
                                    method.Body.Simplify();
                                    method.Body.Optimize();
                                    changed = true;
                                }
                                else if (tz == DialogResult.No)
                                {
                                    continue;
                                }
                                else
                                {
                                    return;
                                }
                            }
                            #endregion
                             * */
                        }
                    }
                }
            }
        }

        static MethodReference MakeGeneric(MethodReference method, TypeReference declaringType)
        {
            var reference = new MethodReference(method.Name, declaringType, method.ReturnType.ReturnType, method.HasThis, method.ExplicitThis, MethodCallingConvention.Generic);
            foreach (ParameterDefinition parameter in method.Parameters)
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
            return reference;
        }

        public void save()
        {
            if (flag) return;
            if (!changed)
            {
                logger.Log("No changes made, skipping save.");
                return;
            }
            logger.Log("Saving Assembly...");
            AssemblyFactory.SaveAssembly(assembly, newLocation);
        }
    }
}
