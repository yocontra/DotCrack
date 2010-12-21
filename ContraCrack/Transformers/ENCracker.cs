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
                WorkingAssembly.MainModule.Types.Cast<TypeDefinition>().Where(type => type.Name != "<Module>"))
            {
                    foreach (MethodDefinition method in type.Methods)
                    {

                        if (method.HasBody && !method.IsAbstract
                            && !method.IsConstructor 
                            && method.ReturnType.FullName.Contains("Boolean") 
                            && method.Parameters.Count == 2 
                            && method.Parameters[0].ParameterType.FullName.Contains("Int32")
                            && method.Parameters[1].ParameterType.FullName.Contains("Int32"))
                        {
                            DialogResult tz = Interface.GetYesNoDialog("Method \"" + type.FullName + '.' + method.Name + "\" has met the search criteria. Crack it?", "Ay Papi!");
                            switch (tz)
                            {
                                case DialogResult.Yes:
                                    {
                                        Logger.Log("Modifying method \"" + type.FullName + '.' + method.Name + "\"");
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
                                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                                        {
                                            worker.Replace(method.Body.Instructions[i], worker.Create(OpCodes.Nop));
                                        }
                                        int count = method.Body.Instructions.Count;
                                        method.Body.ExceptionHandlers.Clear();
                                        method.Body.Variables.Clear();
                                        worker.Replace(method.Body.Instructions[count - 2], worker.Create(OpCodes.Ldc_I4_1));
                                        worker.Replace(method.Body.Instructions[count - 1], worker.Create(OpCodes.Ret));
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
