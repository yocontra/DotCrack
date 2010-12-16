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
                        if (method.HasBody && !method.IsAbstract
                            && !method.IsConstructor
                            && method.ReturnType.ReturnType.FullName.Contains("Void")
                            && method.Parameters.Count == 0 
                            && method.Name == "InitializeComponent")
                        {
                            DialogResult tz = MessageBox.Show("Method \"" + type.FullName + '.' + method.Name + "\" has met the search criteria. Crack it?", "Ay Papi!", MessageBoxButtons.YesNoCancel);
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
                                for (int i = 0; i < method.Body.Instructions.Count - 2; i++)
                                {
                                    if (method.Body.Instructions[i].OpCode == OpCodes.Ldc_I4_0
                                        && method.Body.Instructions[i + 1].OpCode == OpCodes.Callvirt
                                        && method.Body.Instructions[i + 1].Operand.ToString().Contains("set_Enabled"))
                                    {
                                        worker.Replace(method.Body.Instructions[i], worker.Create(OpCodes.Ldc_I4_1));
                                    }
                                }
                                /*int count = method.Body.Instructions.Count;
                                method.Body.ExceptionHandlers.Clear();
                                method.Body.Variables.Clear();
                                worker.Replace(method.Body.Instructions[count - 2], worker.Create(OpCodes.Ldc_I4_1));
                                worker.Replace(method.Body.Instructions[count - 1], worker.Create(OpCodes.Ret));
                                method.Body.ExceptionHandlers.Clear();
                                method.Body.Variables.Clear();
                                method.Body.Simplify();*/
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
