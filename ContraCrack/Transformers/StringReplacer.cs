using System.Linq;
using ContraCrack.Util;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ContraCrack.Transformers
{
    class StringReplacer : ITransformer
    {
        public LogHandler Logger { get; set; }
        public string OriginalLocation { get; set; }
        public string NewLocation { get; set; }
        public AssemblyDefinition OriginalAssembly { get; set; }
        public AssemblyDefinition WorkingAssembly { get; set; }
        public bool HasIssue { get; set; }
        string _toChange = "";
        string _replacement = "";

        public StringReplacer(string fileLoc)
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
            else
            {
                Logger.Log("No Strongname Found!");
            }
            Logger.Log("Gathering User Input...");
            _toChange = Interface.GetUserInputDialog("What string are you replacing?", "Settings", "example.com");
            _replacement = Interface.GetUserInputDialog("What are you replacing it with?", "Settings", "example.net");
        }
        public void Transform()
        {
            foreach (TypeDefinition type in WorkingAssembly.MainModule.Types.Where(type => type.Name != "<Module>"))
            {
                    foreach (MethodDefinition method in type.Methods.Where(method => method.HasBody))
                    {
                        foreach (Instruction t in method.Body.Instructions)
                        {
                            if (t.OpCode != OpCodes.Ldstr || !t.Operand.ToString().Contains(_toChange)) continue;
                            string oldval = t.Operand.ToString();
                            string newval = oldval.Replace(_toChange, _replacement);
                            method.Body.GetILProcessor().Replace(t, method.Body.GetILProcessor().Create(OpCodes.Ldstr, newval));
                            Logger.Log(string.Format("Replaced  \"{0}\" with \"{1}\" in \"{2}{3}{4}\"", oldval, newval, type.FullName, '.', method.Name));
                        }
                    }
            }
        }
        public void Save()
        {
            WorkingAssembly.Write(NewLocation);
        }
    }
}
