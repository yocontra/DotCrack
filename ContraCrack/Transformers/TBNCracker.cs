using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContraCrack.Util;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;
using System.Windows.Forms;

namespace ContraCrack.Transformers
{
    class TBNCracker : Transformer
    {
        LogHandler logger = new LogHandler("TBNCracker");
        string assemblyLocation;
        string newLocation;
        AssemblyDefinition assembly;
        bool flag = false;
        bool changed = false;

        public TBNCracker(string fileLoc)
        {
            logger.Log(logger.Identifier + " Started!");
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
            if (assembly.hasStrongName())
            {
                logger.Log("Removing Strongname Key...");
                assembly.removeStrongName();
            }
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
            transform2();
            return;
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
                            if (method.Body.Instructions.Count >= 6
                                && method.Parameters.Count == 2
                                && method.Body.Instructions[0].OpCode == OpCodes.Ldarg_0
                                && method.Body.Instructions[1].OpCode == OpCodes.Ldc_I4_1
                                && method.Body.Instructions[2].OpCode == OpCodes.Callvirt
                                && method.Body.Instructions[3].OpCode == OpCodes.Ldarg_0
                                && method.Body.Instructions[4].OpCode == OpCodes.Callvirt
                                && method.Body.Instructions[5].OpCode == OpCodes.Ret)
                            {
                                DialogResult tz = Interface.getYesNoDialog("Method \"" + type.FullName + '.' + method.Name + "\" has met the search criteria.\r\n Crack it's Owner's constructor?", "Ay Papi!");
                                if (tz == DialogResult.Yes)
                                {
                                    logger.Log("Injecting method contents of  \"" + type.FullName + '.' + method.Name + "\" into  \"" + type.FullName + '.' + type.Constructors[0].Name + "\"");
                                    int count = method.Body.Instructions.Count;

                                    Instruction ins1 = method.Body.Instructions[count - 2];//Save these for later
                                    Instruction ins2 = method.Body.Instructions[count - 3];
                                    //Create a ghost method that closes the form
                                    TypeReference returnVoidTR = assembly.MainModule.Import(typeof(void));
                                    MethodDefinition closeDef = new MethodDefinition("CloseForm", Mono.Cecil.MethodAttributes.Public, returnVoidTR);
                                    closeDef.Body.CilWorker.Append(ins1);
                                    closeDef.Body.CilWorker.Append(ins2);
                                    closeDef.Body.CilWorker.Append(closeDef.Body.CilWorker.Create(OpCodes.Ret));
                                    //Getting an error here because I add the method to the method collection
                                    //while the foreach is running. will fix later
                                    type.Methods.Add(closeDef);

                                    //Deletes ldarg.0 and replaces this.Close() with this.CloseForm()
                                    method.Body.CilWorker.Remove(method.Body.Instructions[count - 2]);
                                    Instruction newInstr = type.Constructors[0].Body.CilWorker.Create(OpCodes.Callvirt, closeDef);
                                    method.Body.CilWorker.Replace(method.Body.Instructions[count - 3], newInstr);

                                    MethodDefinition newdef = type.Constructors[0];
                                    newdef.Body.CilWorker.Remove(newdef.Body.Instructions[newdef.Body.Instructions.Count - 1]); //Nop out the ret so we can inject our return code
                                    type.Constructors[0] = Util.Cecil.appendMethod(newdef, method);
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
                                DialogResult tz = Interface.getYesNoDialog("Method \"" + type.FullName + '.' + method.Name + "\" has met the search criteria. Patch Form Initializer?", "Ay Papi!");
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
                                        //Enable FUCKING EVERYTHING!!!
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
                            #region auth method
                            /*
                            if (method.HasBody 
                                && method.Name.EndsWith("_Load"))
                                //Needs a better pattern than this lol
                            {
                                DialogResult tz = Interface.getYesNoDialog("Method \"" + type.FullName + '.' + method.Name + "\" has met the search criteria. Crack it?", "Ay Papi!");
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
                            }*/
                            #endregion
                        }
                    }
                }
            }
        }
        public void transform2()
        {
            if (flag) return;
            logger.Log("Starting Transformer...");
            foreach (TypeDefinition type in assembly.MainModule.Types)
            {
                if (type.Name != "<Module>")
                {
                    foreach (MethodDefinition method in type.Methods)
                    {
                        #region removeapplication startup
                        if (method.Name == "MyApplication_Startup")
                        {
                            logger.Log("Removing startup code");
                            method.Body.Instructions[0].OpCode = OpCodes.Ret;
                            changed = true;
                        }
                        #endregion
                        #region enable
                        if (method.Name == "InitializeComponent")
                        {
                            DialogResult tz = Interface.getYesNoDialog("Method \"" + type.FullName + '.' + method.Name + "\" has met the search criteria. Patch Form Initializer?", "Ay Papi!");
                            if (tz == DialogResult.Yes)
                            {
                                logger.Log("Enabling all Controls");
                                for (int i = 0; i < method.Body.Instructions.Count; i++)
                                {
                                    if (method.Body.Instructions.Count <= (i + 1)) break;
                                    //Enabled FUCKING EVERYTHING!!!

                                    if (method.Body.Instructions[i].OpCode == OpCodes.Ldc_I4_0
                                        && method.Body.Instructions[i + 1].OpCode == OpCodes.Callvirt
                                        && method.Body.Instructions[i + 1].Operand.ToString().Contains("set_Enabled"))
                                    {
                                        method.Body.CilWorker.Replace(method.Body.Instructions[i], method.Body.CilWorker.Create(OpCodes.Ldc_I4_1));
                                    }
                                }
                                method.Body.Optimize();
                                changed = true;
                            }
                        }
                        #endregion
                    }
                    foreach (MethodDefinition constructor in type.Constructors)
                    {
                        #region update flags
                        //warning: this is really shitty unclean not vague enough code
                        //todo: make it inject the code from the auth method's success into here
                        if (type.Name == "Form1")
                        {
                            for (int i = 1; i < constructor.Body.Instructions.Count; i++)
                            {
                                if (constructor.Body.Instructions[i].OpCode == OpCodes.Stfld && constructor.Body.Instructions[i].Operand.ToString().Contains("Form1::bS"))
                                {
                                    logger.Log("Patching " + constructor.Body.Instructions[i].Operand.ToString());
                                    constructor.Body.Instructions[i - 1].OpCode = OpCodes.Ldc_I4_1;
                                    changed = true;
                                }
                            }
                        }
                        #endregion
                    }
                }
            }
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
