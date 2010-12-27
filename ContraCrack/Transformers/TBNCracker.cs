using System.Linq;
using ContraCrack.Util;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Windows.Forms;

namespace ContraCrack.Transformers
{
    class TBNCracker : ITransformer
    {
        public LogHandler Logger { get; set; }
        public string OriginalLocation { get; set; }
        public string NewLocation { get; set; }
        public AssemblyDefinition OriginalAssembly { get; set; }
        public AssemblyDefinition WorkingAssembly { get; set; }
        public bool HasIssue { get; set; }

        public TBNCracker(string fileLoc)
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
                    #region remove MyApplication_Startup
                    if (method.Parameters.Count == 2
                        && method.Parameters[0].ParameterType.FullName.Contains("Object")
                        && method.Parameters[1].ParameterType.FullName.Contains("StartupEventArgs"))
                    {
                        DialogResult tz = Interface.GetYesNoDialog(string.Format("Method \"{0}{1}{2}\" contains startup code. Would you like to wipe it?", type.FullName, '.', method.Name), "Ay Papi!");
                        switch (tz)
                        {
                            case DialogResult.Yes:
                                Logger.Log("Removing startup code");
                                method.Body.Instructions.Clear();
                                method.Body.ExceptionHandlers.Clear();
                                method.Body.Variables.Clear();
                                method.Body.Instructions.Add(method.Body.GetILProcessor().Create(OpCodes.Ret));
                                break;
                        }
                    }
                    #endregion
                    #region patch authform_load
                    //this works but the auth form pops up briefly.
                    if (method.HasBody && !method.IsAbstract
                        && !method.IsConstructor
                        && method.ReturnType.FullName.Contains("Void"))
                    {
                        if (method.Body.Instructions.Count >= 6
                            && method.Parameters.Count == 2
                            && method.Body.Instructions[0].OpCode == OpCodes.Ldarg_0
                            && method.Body.Instructions[1].OpCode == OpCodes.Ldc_I4_1
                            && method.Body.Instructions[2].OpCode == OpCodes.Callvirt
                            && method.Body.Instructions[3].OpCode == OpCodes.Ldarg_0
                            && method.Body.Instructions[4].OpCode == OpCodes.Callvirt
                            && method.Body.Instructions[5].OpCode == OpCodes.Ret)
                        {
                            DialogResult tz = Interface.GetYesNoDialog(string.Format("Method \"{0}{1}{2}\" has met the search criteria.\r\n Crack it's Owner's form load?", type.FullName, '.', method.Name), "Ay Papi!");
                            switch (tz)
                            {
                                case DialogResult.Yes:
                                    {
                                        MethodDefinition construct = type.GetConstructors()[0];
                                        MethodReference formload = (from t in construct.Body.Instructions where t.OpCode == OpCodes.Ldvirtftn select (MethodReference)t.Operand).FirstOrDefault();
                                        if (formload != null)
                                        {
                                            Logger.Log(string.Format("Injecting method contents of  \"{0}{1}{2}\" into  \"{3}{4}{5}\"", type.FullName, '.', method.Name, type.FullName, '.', formload.Name));
                                            MethodDefinition formloadDef = type.Methods.Where(methodt => methodt.Name == formload.Name).FirstOrDefault();
                                            formloadDef.Body.Instructions.Clear();
                                            formloadDef.AppendMethod(method);
                                        }
                                        else
                                        {
                                            Logger.Log("Found buttonClick pattern but could not find form_load in form constructor.");
                                        }
                                    }
                                    break;
                            }
                        }
                    }

                    #endregion
                    #region patch initializecomponent
                    //This enables everything on the form
                    if (!method.IsConstructor
                        && method.ReturnType.FullName.Contains("Void")
                        && method.Parameters.Count == 0
                        && method.Body.Variables.Count >= 1
                        && method.Body.Variables[0].VariableType.FullName.Contains("ComponentResourceManager"))
                    {
                        DialogResult tz = Interface.GetYesNoDialog(string.Format("Method \"{0}{1}{2}\" has met the search criteria. Patch Form Initializer?", type.FullName, '.', method.Name), "Ay Papi!");
                        switch (tz)
                        {
                            case DialogResult.Yes:
                                {
                                    Logger.Log(string.Format("Modifying method \"{0}{1}{2}\"", type.FullName, '.', method.Name));
                                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                                    {
                                        if (method.Body.Instructions[i].OpCode == OpCodes.Ldc_I4_0
                                            && method.Body.Instructions[i + 1].OpCode == OpCodes.Callvirt
                                            && method.Body.Instructions[i + 1].Operand.ToString().Contains("set_Enabled"))
                                        {
                                            method.Body.Instructions[i] = method.Body.GetILProcessor().Create(OpCodes.Ldc_I4_1);
                                        }
                                    }
                                }
                                break;
                        }
                    }

                    #endregion
                }
            }
        }

        public void Save()
        {
            WorkingAssembly.Write(NewLocation);
        }
    }
}
