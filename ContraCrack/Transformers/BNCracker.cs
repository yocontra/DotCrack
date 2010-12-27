using System.Linq;
using ContraCrack.Util;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Windows.Forms;

namespace ContraCrack.Transformers
{
    class BNCracker : ITransformer
    {
        public LogHandler Logger { get; set; }
        public string OriginalLocation { get; set; }
        public string NewLocation { get; set; }
        public AssemblyDefinition OriginalAssembly { get; set; }
        public AssemblyDefinition WorkingAssembly { get; set; }
        public bool HasIssue { get; set; }

        public BNCracker(string fileLoc)
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
                foreach (MethodDefinition method in type.Methods.Where(method => method.HasBody))
                    {
                        if (method.HasBody && !method.IsAbstract
                            && !method.IsConstructor
                            && method.ReturnType.FullName.Contains("String") 
                            && method.Parameters.Count == 4
                            && method.Parameters[0].ParameterType.FullName.Contains("String")
                            && method.Parameters[1].ParameterType.FullName.Contains("String")
                            && method.Parameters[2].ParameterType.FullName.Contains("String")
                            && method.Parameters[3].ParameterType.FullName.Contains("Int32"))
                        {
                            DialogResult tz = Interface.GetYesNoDialog(string.Format("Method \"{0}{1}{2}\" has met the search criteria. Crack it?", type.FullName, '.', method.Name), "Ay Papi!");
                            switch (tz)
                            {
                                case DialogResult.Yes:
                                    {
                                        Logger.Log(string.Format("Modifying method \"{0}{1}{2}\"", type.FullName, '.', method.Name));
                                        if (method.Body.Instructions[0].Operand.ToString() != "authentication.bottingnation.com")
                                        {
                                            Logger.Log(string.Format("Couldn't find auth string in method \"{0}{1}{2}\"", type.FullName, '.', method.Name));
                                            //this will only work on assemblies without string obfuscation :( will add manual override later
                                            continue;
                                        }
                                        string returnVal = "";
                                        //We find the correct return ;)
                                        for(int i = 0; i < method.Body.Instructions.Count; i++){
                                            if((i + 1) >= method.Body.Instructions.Count) break;
                                            if (method.Body.Instructions[i].OpCode == OpCodes.Bne_Un_S && method.Body.Instructions[i + 1].OpCode == OpCodes.Ldstr)
                                            {
                                                returnVal = method.Body.Instructions[i + 1].Operand.ToString();
                                                break;
                                            }
                                        }
                                        if (returnVal == "")
                                        {
                                            //Couldn't find return val, just skip this method
                                            Logger.Log(string.Format("Couldn't find instruction pattern in method \"{0}{1}{2}\"", type.FullName, '.', method.Name));
                                            continue;
                                        }
                                        //We found the pattern and have the return value, now lets wipe everything and ret it
                                        method.Clear();
                                        method.Body.Instructions.Add(method.Body.GetILProcessor().Create(OpCodes.Ldstr, returnVal));
                                        method.Body.Instructions.Add(method.Body.GetILProcessor().Create(OpCodes.Ret));
                                    }
                                    break;
                            }
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
