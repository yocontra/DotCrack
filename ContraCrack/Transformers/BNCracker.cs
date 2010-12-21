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
            Logger.Log(Logger.Identifier + " Started!");
            OriginalLocation = fileLoc;
            NewLocation = OriginalLocation.GetNewFileName();
        }
        public void Load()
        {
            try
            {
                OriginalAssembly = AssemblyFactory.GetAssembly(OriginalLocation);
                WorkingAssembly = OriginalAssembly;
            }
            catch (Exception)
            {
                Logger.Log(Util.Constants.AssemblyErrorMessage);
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
            Logger.Log("Starting Transformer...");
            foreach (TypeDefinition type in
                WorkingAssembly.MainModule.Types.Cast<TypeDefinition>().Where(type => type.Name != "<Module>"))
            {
                    foreach (MethodDefinition method in type.Methods)
                    {
                        if (method.HasBody && !method.IsAbstract
                            && !method.IsConstructor
                            && method.ReturnType.ReturnType.FullName.Contains("String") 
                            && method.Parameters.Count == 4
                            && method.Parameters[0].ParameterType.FullName.Contains("String")
                            && method.Parameters[1].ParameterType.FullName.Contains("String")
                            && method.Parameters[2].ParameterType.FullName.Contains("String")
                            && method.Parameters[3].ParameterType.FullName.Contains("Int32"))
                        {
                            DialogResult tz = Interface.GetYesNoDialog("Method \"" + type.FullName + '.' + method.Name + "\" has met the search criteria. Crack it?", "Ay Papi!");
                            switch (tz)
                            {
                                case DialogResult.Yes:
                                    {
                                        Logger.Log("Modifying method \"" + type.FullName + '.' + method.Name + "\"");
                                        CilWorker worker;
                                        try
                                        {
                                            worker = method.Body.CilWorker;
                                        }
                                        catch (Exception)
                                        {
                                            Logger.Log(Util.Constants.MSILErrorMessage);
                                            HasIssue = true;
                                            return;
                                        }
                                        if (method.Body.Instructions[0].Operand.ToString() != "authentication.bottingnation.com")
                                        {
                                            Logger.Log("Couldn't find auth string in method \"" + type.FullName + '.' + method.Name + "\"");
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
                                            Logger.Log("Couldn't find instruction pattern in method \"" + type.FullName + '.' + method.Name + "\"");
                                            continue;
                                        }
                                        //We found the pattern and have the return value, now lets wipe everything and ret it
                                        for (int i = 0; i < method.Body.Instructions.Count - 2; i++)
                                        {
                                            worker.Replace(method.Body.Instructions[i], worker.Create(OpCodes.Nop));
                                        }
                                        int count = method.Body.Instructions.Count;
                                        method.Body.ExceptionHandlers.Clear();
                                        method.Body.Variables.Clear();
                                        worker.Replace(method.Body.Instructions[count - 2], worker.Create(OpCodes.Ldstr, returnVal));//Manual: A7u-_i4-#~=w2_O5$42-_&3_0
                                        worker.Replace(method.Body.Instructions[count - 1], worker.Create(OpCodes.Ret));
                                        method.Body.Simplify();
                                        method.Body.Optimize();
                                    }
                                    break;
                            }
                        }
                    }
            }
        }
        public void Save()
        {
            AssemblyFactory.SaveAssembly(WorkingAssembly, NewLocation);
        }
    }
}
