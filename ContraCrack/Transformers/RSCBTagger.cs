using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;
using System.Windows.Forms;
using ContraCrack.Util;

namespace ContraCrack.Transformers
{
    class RSCBTagger : Transformer
    {
        LogHandler logger = new LogHandler("RSCBTagger");
        string assemblyLocation;
        string newLocation;
        AssemblyDefinition assembly;
        bool flag = false;

        public RSCBTagger(string fileLoc)
        {
            logger.Log(logger.Identifier + " Started!");
            assemblyLocation = fileLoc;
            newLocation = fileLoc.Replace(".exe", "-tagged.exe");
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
                        if (method == assembly.EntryPoint)
                        {
                            logger.Log("Injecting code into entrypoint \"" + type.FullName + '.' + method.Name + "\"");
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
                            MethodInfo showMessageMethod = typeof(System.Windows.Forms.MessageBox).GetMethod("Show", new Type[] { typeof(string) });
                            MethodReference showMessageBox = assembly.MainModule.Import(showMessageMethod);
                            Instruction insertSentence = worker.Create(OpCodes.Ldstr, "Cracked by RSCBUnlocked.net");
                            Instruction callShowMessage = worker.Create(OpCodes.Call, showMessageBox);
                            worker.InsertBefore(method.Body.Instructions[0], insertSentence);
                            worker.InsertAfter(insertSentence, callShowMessage);
                            worker.InsertAfter(callShowMessage, worker.Create(OpCodes.Pop));
                        }
                    }
                }
            }
        }
        public void save()
        {
            if (flag) return;
            logger.Log("Saving Assembly...");
            AssemblyFactory.SaveAssembly(assembly, newLocation);
        }
    }
}
