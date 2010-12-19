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
    class ENCracker : Transformer
    {
        LogHandler logger = new LogHandler("ENCracker");
        string assemblyLocation;
        string newLocation;
        AssemblyDefinition assembly;
        public bool flag { get; set; }
        public bool changed { get; set; }

        public ENCracker(string fileLoc)
        {
            logger.Log(logger.Identifier + " Started!");
            assemblyLocation = fileLoc;
            newLocation = fileLoc.Replace(".exe", "-cracked.exe");
        }
        public void load()
        {
            logger.Log("Loading Assembly...");
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
        public void transform()
        {
            logger.Log("Starting Transformer...");
            foreach (TypeDefinition type in assembly.MainModule.Types)
            {
                if (type.Name != "<Module>")
                {
                    foreach (MethodDefinition method in type.Methods)
                    {

                        if (method.HasBody && !method.IsAbstract
                            && !method.IsConstructor 
                            && method.ReturnType.ReturnType.FullName.Contains("Boolean") 
                            && method.Parameters.Count == 2 
                            && method.Parameters[0].ParameterType.FullName.Contains("Int32")
                            && method.Parameters[1].ParameterType.FullName.Contains("Int32"))
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
                                    worker.Replace(method.Body.Instructions[i], worker.Create(OpCodes.Nop));
                                }
                                int count = method.Body.Instructions.Count;
                                method.Body.ExceptionHandlers.Clear();
                                method.Body.Variables.Clear();
                                worker.Replace(method.Body.Instructions[count - 2], worker.Create(OpCodes.Ldc_I4_1));
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
                    }
                }
            }
        }
        public void save()
        {
            logger.Log("Saving Assembly...");
            AssemblyFactory.SaveAssembly(assembly, newLocation);
        }
    }
}
