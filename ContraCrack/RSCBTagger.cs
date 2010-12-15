using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;
using System.Windows.Forms;

namespace ContraCrack
{
    class RSCBTagger : Transformer
    {
        string assemblyLocation;
        string newLocation;
        AssemblyDefinition assembly;

        public RSCBTagger(string fileLoc)
        {
            assemblyLocation = fileLoc;
            newLocation = fileLoc.Replace(".exe", "-tagged.exe");
        }
        public void load()
        {
            assembly = AssemblyFactory.GetAssembly(assemblyLocation);
            assembly.Name.PublicKey = new byte[0];
            assembly.Name.PublicKeyToken = new byte[0];
            assembly.Name.Flags = AssemblyFlags.SideBySideCompatible;
        }
        public void transform()
        {
            foreach (TypeDefinition type in assembly.MainModule.Types)
            {
                if (type.Name != "<Module>")
                {
                    foreach (MethodDefinition method in type.Methods)
                    {
                        if (method == assembly.EntryPoint)
                        {
                            MethodInfo showMessageMethod = typeof(System.Windows.Forms.MessageBox).GetMethod("Show", new Type[] { typeof(string) });
                            CilWorker worker = method.Body.CilWorker;
                            MethodReference showMessageBox = assembly.MainModule.Import(showMessageMethod);
                            Instruction insertSentence = worker.Create(OpCodes.Ldstr, "Cracked by RSCBUnlocked.net");
                            Instruction callShowMessage = worker.Create(OpCodes.Call, showMessageBox);
                            method.Body.CilWorker.InsertBefore(method.Body.Instructions[0], insertSentence);
                            worker.InsertAfter(insertSentence, callShowMessage);
                            worker.InsertAfter(callShowMessage, worker.Create(OpCodes.Pop));
                        }
                    }
                }
            }
        }
        public void save()
        {
            AssemblyFactory.SaveAssembly(assembly, newLocation);
        }
    }
}
