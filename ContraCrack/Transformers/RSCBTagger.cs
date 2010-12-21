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
        public LogHandler Logger { get; set; }
        public string OriginalLocation { get; set; }
        public string NewLocation { get; set; }
        public AssemblyDefinition OriginalAssembly { get; set; }
        public AssemblyDefinition WorkingAssembly { get; set; }
        public bool HasIssue { get; set; }

        public RSCBTagger(string fileLoc)
        {
            Logger = new LogHandler(GetType().Name);
            Logger.Log(Logger.Identifier + " Initialized.");
            OriginalLocation = fileLoc;
            NewLocation = OriginalLocation.GetNewFileName();
        }
        public void Load()
        {
            try
            {
                OriginalAssembly = AssemblyDefinition.ReadAssembly(OriginalLocation);
                WorkingAssembly = OriginalAssembly;
            }
            catch
            {
                Logger.Log(Constants.AssemblyErrorMessage);
                HasIssue = true;
                return;
            }
            if (WorkingAssembly.HasStrongName())
            {
                Logger.Log("Removing Strongname Key...");
                WorkingAssembly.RemoveStrongName();
            }
        }
        public void Transform()
        {
            foreach (TypeDefinition type in
                WorkingAssembly.MainModule.Types.Cast<TypeDefinition>().Where(type => type.Name != "<Module>"))
            {
                    foreach (MethodDefinition method in
                        type.Methods.Cast<MethodDefinition>().Where(method => method == WorkingAssembly.EntryPoint))
                    {
                        Logger.Log("Injecting code into entrypoint \"" + type.FullName + '.' + method.Name + "\"");
                        ILProcessor worker;
                        try
                        {
                            worker = method.Body.GetILProcessor();
                        }
                        catch
                        {
                            Logger.Log(Constants.MSILErrorMessage);
                            HasIssue = true;
                            return;
                        }
                        MethodInfo showMessageMethod = typeof(MessageBox).GetMethod("Show", new[] { typeof(string) });
                        MethodReference showMessageBox = WorkingAssembly.MainModule.Import(showMessageMethod);
                        Instruction insertSentence = worker.Create(OpCodes.Ldstr, "Cracked by RSCBUnlocked.net");
                        Instruction callShowMessage = worker.Create(OpCodes.Call, showMessageBox);
                        worker.InsertBefore(method.Body.Instructions[0], insertSentence);
                        worker.InsertAfter(insertSentence, callShowMessage);
                        worker.InsertAfter(callShowMessage, worker.Create(OpCodes.Pop));
                    }
            }
        }
        public void Save()
        {
            WorkingAssembly.Write(NewLocation);
        }
    }
}
