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
    class StringReplacer : Transformer
    {
        LogHandler logger = new LogHandler("StringReplacer");
        string assemblyLocation;
        string newLocation;
        AssemblyDefinition assembly;
        bool flag = false; 
        bool changed = false;
        string toChange = "";
        string replacement = "";

        public StringReplacer(string fileLoc)
        {
            logger.Log(logger.Identifier + " Started!");
            assemblyLocation = fileLoc;
            newLocation = fileLoc.Replace(".exe", "-cracked.exe");
        }
        public void load()
        {
            logger.Log("Loading Assembly...");
            if (flag) return;
            try
            {
                assembly = AssemblyFactory.GetAssembly(assemblyLocation);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error loading assembly.");
                flag = true;
                return;
            }
            if (assembly.hasStrongName())
            {
                logger.Log("Removing Strongname Key...");
                assembly.removeStrongName();
            }
            logger.Log("Gathering User Input...");
            toChange = Interface.getUserInputDialog("What string are you replacing?", "Settings", "example.com");
            replacement = Interface.getUserInputDialog("What are you replacing it with?", "Settings", "example.net");
        }
        public void transform()
        {
            if (flag) return;
            logger.Log("Starting Transformer...");
            foreach (TypeDefinition type in assembly.MainModule.Types)
            {
                if (type.Name != "<Module>")
                {
                    foreach (MethodDefinition method in type.Methods)
                    {
                        if (method.HasBody)
                        {
                                CilWorker worker;
                                try
                                {
                                    worker = method.Body.CilWorker;
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show("Issue reading MSIL. Assembly is obfuscated or corrupt.");
                                    flag = true;
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
                                changed = true;
                        }
                    }
                }
            }
        }
        public void save()
        {
            if (flag) return;
            if (!changed)
            {
                logger.Log("No changes made, skipping save.");
                return;
            }
            logger.Log("Saving Assembly...");
            AssemblyFactory.SaveAssembly(assembly, newLocation);
        }
    }
}
