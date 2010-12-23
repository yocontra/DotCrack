using System;
using System.Linq;
using ContraCrack.Util;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Windows.Forms;

namespace ContraCrack.Transformers
{
    class ENCracker : ITransformer
    {
        public LogHandler Logger { get; set; }
        public string OriginalLocation { get; set; }
        public string NewLocation { get; set; }
        public AssemblyDefinition OriginalAssembly { get; set; }
        public AssemblyDefinition WorkingAssembly { get; set; }
        public bool HasIssue { get; set; }

        public ENCracker(string fileLoc)
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

                        if (!method.IsAbstract
                            && !method.IsConstructor 
                            && method.ReturnType.FullName.Contains("Boolean") 
                            && method.Parameters.Count == 2 
                            && method.Parameters[0].ParameterType.FullName.Contains("Int32")
                            && method.Parameters[1].ParameterType.FullName.Contains("Int32"))
                        {
                            DialogResult tz = Interface.GetYesNoDialog(string.Format("Method \"{0}{1}{2}\" has met the search criteria. Crack it?", type.FullName, '.', method.Name), "Ay Papi!");
                            switch (tz)
                            {
                                case DialogResult.Yes:
                                    {
                                        Logger.Log(string.Format("Modifying method \"{0}{1}{2}\"", type.FullName, '.', method.Name));
                                        //Wipe the method whilst avoiding method.body.instructions for obfuscation purposes
                                        MethodDefinition choni = new MethodDefinition(method.Name, method.Attributes, method.ReturnType);
                                        method.Body = choni.Body;
                                        method.Body.Instructions.Add(method.Body.GetILProcessor().Create(OpCodes.Ldc_I4_1));
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
