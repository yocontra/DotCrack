using System.Linq;
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
                WorkingAssembly.MainModule.Types.Where(type => type.Name != "<Module>"))
            {
                    foreach (MethodDefinition method in
                        type.Methods.Where(method => method == WorkingAssembly.EntryPoint))
                    {
                        Logger.Log(string.Format("Injecting code into entrypoint \"{0}{1}{2}\"", type.FullName, '.', method.Name));
                        MethodReference showMessageBox = WorkingAssembly.MainModule.Import(typeof(MessageBox).GetMethod("Show", new[] { typeof(string) }));
                        Instruction insertSentence = method.Body.GetILProcessor().Create(OpCodes.Ldstr, "Cracked by RSCBUnlocked.net");
                        Instruction callShowMessage = method.Body.GetILProcessor().Create(OpCodes.Call, showMessageBox);
                        method.Body.GetILProcessor().InsertBefore(method.Body.Instructions[0], insertSentence);
                        method.Body.GetILProcessor().InsertAfter(insertSentence, callShowMessage);
                        method.Body.GetILProcessor().InsertAfter(callShowMessage, method.Body.GetILProcessor().Create(OpCodes.Pop));
                    }
            }
        }
        public void Save()
        {
            WorkingAssembly.Write(NewLocation);
        }
    }
}
