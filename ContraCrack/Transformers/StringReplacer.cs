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
    class StringReplacer : ITransformer
    {
        public LogHandler Logger { get; set; }
        public string OriginalLocation { get; set; }
        public string NewLocation { get; set; }
        public AssemblyDefinition OriginalAssembly { get; set; }
        public AssemblyDefinition WorkingAssembly { get; set; }
        public bool HasIssue { get; set; }
        string toChange = "";
        string replacement = "";

        public StringReplacer(string fileLoc)
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
            Logger.Log("Gathering User Input...");
            toChange = Interface.GetUserInputDialog("What string are you replacing?", "Settings", "example.com");
            replacement = Interface.GetUserInputDialog("What are you replacing it with?", "Settings", "example.net");
        }
        public void Transform()
        {
            Logger.Log("Starting Transformer...");
            foreach (TypeDefinition type in
                WorkingAssembly.MainModule.Types.Cast<TypeDefinition>().Where(type => type.Name != "<Module>"))
            {
                    foreach (MethodDefinition method in
                        type.Methods.Cast<MethodDefinition>().Where(method => method.HasBody))
                    {
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
                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            if(method.Body.Instructions[i].OpCode == OpCodes.Ldstr
                               && method.Body.Instructions[i].Operand.ToString().Contains(toChange)){
                                   string oldval = method.Body.Instructions[i].Operand.ToString();
                                   string newval = oldval.Replace(toChange, replacement);
                                   worker.Replace(method.Body.Instructions[i], worker.Create(OpCodes.Ldstr, newval));
                                   Logger.Log("Replaced  \"" + oldval 
                                              + "\" with \"" + newval + "\" in \"" + type.FullName + '.' + method.Name + "\"");
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
