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
        LogHandler logger = new LogHandler("StringReplacer");
        string assemblyLocation;
        string newLocation;
        AssemblyDefinition assembly;
        public bool Flag { get; set; }
        public bool Changed { get; set; }
        string toChange = "";
        string replacement = "";

        public StringReplacer(string fileLoc)
        {
            logger.Log(logger.Identifier + " Started!");
            assemblyLocation = fileLoc;
            newLocation = fileLoc.Replace(".exe", "-cracked.exe");
        }
        public void Load()
        {
            logger.Log("Loading Assembly...");
            try
            {
                assembly = AssemblyFactory.GetAssembly(assemblyLocation);
            }
            catch (Exception)
            {
                MessageBox.Show("Error loading assembly.");
                Flag = true;
                return;
            }
            if (assembly.HasStrongName())
            {
                logger.Log("Removing Strongname Key...");
                assembly.RemoveStrongName();
            }
            logger.Log("Gathering User Input...");
            toChange = Interface.GetUserInputDialog("What string are you replacing?", "Settings", "example.com");
            replacement = Interface.GetUserInputDialog("What are you replacing it with?", "Settings", "example.net");
        }
        public void Transform()
        {
            logger.Log("Starting Transformer...");
            foreach (TypeDefinition type in assembly.MainModule.Types)
            {
                if (type.Name != "<Module>")
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
                            MessageBox.Show("Issue reading MSIL. Assembly is obfuscated or corrupt.");
                            Flag = true;
                            return;
                        }
                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            if(method.Body.Instructions[i].OpCode == OpCodes.Ldstr
                               && method.Body.Instructions[i].Operand.ToString().Contains(toChange)){
                                   string oldval = method.Body.Instructions[i].Operand.ToString();
                                   string newval = oldval.Replace(toChange, replacement);
                                   worker.Replace(method.Body.Instructions[i], worker.Create(OpCodes.Ldstr, newval));
                                   logger.Log("Replaced  \"" + oldval 
                                              + "\" with \"" + newval + "\" in \"" + type.FullName + '.' + method.Name + "\"");
                               }
                        }
                        Changed = true;
                    }
                }
            }
        }
        public void Save()
        {
            logger.Log("Saving Assembly...");
            AssemblyFactory.SaveAssembly(assembly, newLocation);
        }
    }
}
