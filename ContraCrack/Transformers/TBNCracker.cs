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
    class TBNCracker : ITransformer
    {
        LogHandler logger = new LogHandler("TBNCracker");
        string assemblyLocation;
        string newLocation;
        AssemblyDefinition assembly;
        public bool Flag { get; set; }
        public bool Changed { get; set; }

        public TBNCracker(string fileLoc)
        {
            logger.Log(logger.Identifier + " Started!");
            assemblyLocation = fileLoc;
            newLocation = fileLoc.Replace(".exe", "-cracked.exe");
        }
        public void Load()
        {
            logger.Log("Loading Assembly...");
            try
            {
                assembly = AssemblyFactory.GetAssembly(assemblyLocation);
            }
            catch (Exception)
            {
                MessageBox.Show("Error loading assembly.");
                Flag = true;
                return;
            }
            if (assembly.HasStrongName())
            {
                logger.Log("Removing Strongname Key...");
                assembly.RemoveStrongName();
            }
        }
        public void Transform()
        {
            //transform2();
            //return;
            logger.Log("Starting Transformer...");
            foreach (TypeDefinition type in assembly.MainModule.Types)
            {
                if (type.Name != "<Module>")
                {
                    foreach (MethodDefinition method in type.Methods)
                    {
                        #region remove MyApplication_Startup
                        //TODO: This needs a pattern
                        if (method.Name == "MyApplication_Startup")
                        {
                            DialogResult tz = Interface.GetYesNoDialog("Method \"" + type.FullName + '.' + method.Name + "\" contains startup code. Would you like to wipe it?", "Ay Papi!");
                            switch (tz)
                            {
                                case DialogResult.Yes:
                                    logger.Log("Removing startup code");
                                    method.Body.Instructions[0].OpCode = OpCodes.Ret;
                                    Changed = true;
                                    break;
                            }
                        }

                        #endregion
                        #region patch authform_load
                        //this works but the auth form pops up briefly.
                        if (method.HasBody && !method.IsAbstract
                            && !method.IsConstructor
                            && method.ReturnType.ReturnType.FullName.Contains("Void"))
                        {
                            if (method.Body.Instructions.Count >= 6
                                && method.Parameters.Count == 2
                                && method.Body.Instructions[0].OpCode == OpCodes.Ldarg_0
                                && method.Body.Instructions[1].OpCode == OpCodes.Ldc_I4_1
                                && method.Body.Instructions[2].OpCode == OpCodes.Callvirt
                                && method.Body.Instructions[3].OpCode == OpCodes.Ldarg_0
                                && method.Body.Instructions[4].OpCode == OpCodes.Callvirt
                                && method.Body.Instructions[5].OpCode == OpCodes.Ret)
                            {
                                DialogResult tz = Interface.GetYesNoDialog("Method \"" + type.FullName + '.' + method.Name + "\" has met the search criteria.\r\n Crack it's Owner's form load?", "Ay Papi!");
                                switch (tz)
                                {
                                    case DialogResult.Yes:
                                        {
                                            MethodDefinition construct = type.Constructors[0];
                                            MethodReference formload = null;
                                            for (int y = 0; y < construct.Body.Instructions.Count; y++)
                                            {
                                                if (construct.Body.Instructions[y].OpCode == OpCodes.Ldvirtftn)
                                                {
                                                    formload = (MethodReference)construct.Body.Instructions[y].Operand;
                                                    break;
                                                }
                                            }
                                            if (formload != null)
                                            {
                                                logger.Log("Injecting method contents of  \"" + type.FullName + '.' + method.Name + "\" into  \"" + type.FullName + '.' + formload.Name + "\"");
                                                MethodDefinition formloadDef = type.Methods.GetMethod(formload.Name)[0];
                                                formloadDef.Body.Instructions.Clear();
                                                formloadDef.AppendMethod(method);
                                                Changed = true;
                                            }
                                            else
                                            {
                                                logger.Log("Found buttonClick pattern but could not find form_load in form constructor.");
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                        #endregion
                        #region patch authform constructor
                        //Gets the contents of the button click method (where it returns the dialogresult and closes)
                        //and append it to the end of the form constructor.
                        //This shit doesn't work, apparently you can't have this.Close() in a constructor.
                        if (method.HasBody && !method.IsAbstract
                            && !method.IsConstructor
                            && method.ReturnType.ReturnType.FullName.Contains("Void"))
                        {
                            if (method.Body.Instructions.Count >= 6
                                && method.Parameters.Count == 2
                                && method.Body.Instructions[0].OpCode == OpCodes.Ldarg_0
                                && method.Body.Instructions[1].OpCode == OpCodes.Ldc_I4_1
                                && method.Body.Instructions[2].OpCode == OpCodes.Callvirt
                                && method.Body.Instructions[3].OpCode == OpCodes.Ldarg_0
                                && method.Body.Instructions[4].OpCode == OpCodes.Callvirt
                                && method.Body.Instructions[5].OpCode == OpCodes.Ret)
                            {
                                DialogResult tz = Interface.GetYesNoDialog("Method \"" + type.FullName + '.' + method.Name + "\" has met the search criteria.\r\n Crack it's Owner's constructor?", "Ay Papi!");
                                switch (tz)
                                {
                                    case DialogResult.Yes:
                                        {
                                            logger.Log("Injecting method contents of  \"" + type.FullName + '.' + method.Name + "\" into  \"" + type.FullName + '.' + type.Constructors[0].Name + "\"");
                                            MethodDefinition newdef = type.Constructors[0];
                                            type.Constructors[0] = newdef.AppendMethod(method);
                                            type.Constructors[0].Body.Optimize();
                                            Changed = true;
                                        }
                                        break;
                                }
                            }
                        }
                        #endregion
                        #region patch initializecomponent
                        //This enables everything on the form
                        if (method.HasBody && !method.IsAbstract
                                && !method.IsConstructor
                                && method.IsPrivate
                                && method.ReturnType.ReturnType.FullName.Contains("Void")
                                && method.Parameters.Count == 0
                                && method.Body.Variables.Count >= 1
                                && method.Body.Variables[0].VariableType.FullName.Contains("ComponentResourceManager"))
                        {
                            DialogResult tz = Interface.GetYesNoDialog("Method \"" + type.FullName + '.' + method.Name + "\" has met the search criteria. Patch Form Initializer?", "Ay Papi!");
                            switch (tz)
                            {
                                case DialogResult.Yes:
                                    {
                                        logger.Log("Modifying method \"" + type.FullName + '.' + method.Name + "\"");
                                        CilWorker worker;
                                        try
                                        {
                                            worker = method.Body.CilWorker;
                                        }
                                        catch (Exception)
                                        {
                                            MessageBox.Show("Issue reading MSIL. Assembly is obfuscated or corrupt.");
                                            Flag = true;
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
                                        Changed = true;
                                    }
                                    break;
                            }
                        }

                        #endregion
                        }
                    }
                }
            }
        public void Transform2()
        {
            logger.Log("Starting Transformer...");
            foreach (TypeDefinition type in
                assembly.MainModule.Types.Cast<TypeDefinition>().Where(type => type.Name != "<Module>"))
            {
                foreach (MethodDefinition method in type.Methods)
                {
                    #region removeapplication startup
                    if (method.Name == "MyApplication_Startup")
                    {
                        logger.Log("Removing startup code");
                        method.Body.Instructions[0].OpCode = OpCodes.Ret;
                        Changed = true;
                    }
                    #endregion
                    #region enable
                    if (method.Name == "InitializeComponent")
                    {
                        DialogResult tz = Interface.GetYesNoDialog("Method \"" + type.FullName + '.' + method.Name + "\" has met the search criteria. Patch Form Initializer?", "Ay Papi!");
                        switch (tz)
                        {
                            case DialogResult.Yes:
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
                                Changed = true;
                                break;
                        }
                    }

                    #endregion
                }
                foreach (MethodDefinition constructor in
                    type.Constructors.Cast<MethodDefinition>().Where(constructor => type.Name == "Form1"))
                {
                    for (int i = 1; i < constructor.Body.Instructions.Count; i++)
                    {
                        if (constructor.Body.Instructions[i].OpCode == OpCodes.Stfld &&
                            constructor.Body.Instructions[i].Operand.ToString().Contains("Form1::bS"))
                        {
                            logger.Log("Patching " + constructor.Body.Instructions[i].Operand);
                            constructor.Body.Instructions[i - 1].OpCode = OpCodes.Ldc_I4_1;
                            Changed = true;
                        }
                    }
                }
            }
        }

        public void Save()
        {
            logger.Log("Saving Assembly...");
            AssemblyFactory.SaveAssembly(assembly, newLocation);
        }
    }
}
