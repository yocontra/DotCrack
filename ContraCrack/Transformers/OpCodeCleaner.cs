using System;
using ContraCrack.Util;
using Mono.Cecil;

namespace ContraCrack.Transformers
{
    class OpCodeCleaner : ITransformer
    {
        public LogHandler Logger { get; set; }
        public string OriginalLocation { get; set; }
        public string NewLocation { get; set; }
        public AssemblyDefinition OriginalAssembly { get; set; }
        public AssemblyDefinition WorkingAssembly { get; set; }
        public bool HasIssue { get; set; }

        public OpCodeCleaner(string fileLoc)
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
            Logger.Log("Gathering User Input...");
        }
        public void Transform()
        {
            foreach (TypeDefinition type in WorkingAssembly.MainModule.Types)
            {
                int replaced = 0;
                foreach (MethodDefinition method in type.Methods)
                {
                    replaced += method.RemoveInvalidOpCodes();
                }
                if (replaced > 0)
                {
                    Logger.Log("NOPed " + replaced + " OpCodes in " + type.FullName);
                }
            }
        }
        public void Save()
        {
            try
            {
                WorkingAssembly.Write(NewLocation);
            } catch (Exception e)
            {
                Logger.Log(e.Message);
                Logger.Log(e.StackTrace);
            }
        }
    }
}
