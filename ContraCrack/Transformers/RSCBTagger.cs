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
    class RSCBTagger : ITransformer
    {
        LogHandler logger = new LogHandler("RSCBTagger");
        string assemblyLocation;
        string newLocation;
        AssemblyDefinition assembly;
        public bool Flag { get; set; }
        public bool Changed { get; set; }

        public RSCBTagger(string fileLoc)
        {
            logger.Log(logger.Identifier + " Started!");
            assemblyLocation = fileLoc;
            newLocation = fileLoc.Replace(".exe", "-tagged.exe");
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
                            catch (Exception)
                            {
                                MessageBox.Show("Issue reading MSIL. Assembly is obfuscated or corrupt.");
                                Flag = true;
                                return;
                            }
                            MethodInfo showMessageMethod = typeof(MessageBox).GetMethod("Show", new[] { typeof(string) });
                            MethodReference showMessageBox = assembly.MainModule.Import(showMessageMethod);
                            Instruction insertSentence = worker.Create(OpCodes.Ldstr, "Cracked by RSCBUnlocked.net");
                            Instruction callShowMessage = worker.Create(OpCodes.Call, showMessageBox);
                            worker.InsertBefore(method.Body.Instructions[0], insertSentence);
                            worker.InsertAfter(insertSentence, callShowMessage);
                            worker.InsertAfter(callShowMessage, worker.Create(OpCodes.Pop));
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
