﻿using System;
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
        LogHandler logger = new LogHandler("BNCracker");
        string assemblyLocation;
        string newLocation;
        AssemblyDefinition assembly;
        public bool Flag { get; set; }
        public bool Changed { get; set; }

        public BNCracker(string fileLoc)
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
        }
        public void Transform()
        {
            logger.Log("Starting Transformer...");
            foreach (TypeDefinition type in assembly.MainModule.Types)
            {
                if (type.Name != "<Module>")
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
                                        logger.Log("Modifying method \"" + type.FullName + '.' + method.Name + "\"");
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
                                        if (method.Body.Instructions[0].Operand.ToString() != "authentication.bottingnation.com")
                                        {
                                            logger.Log("Couldn't find auth string in method \"" + type.FullName + '.' + method.Name + "\"");
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
                                            logger.Log("Couldn't find instruction pattern in method \"" + type.FullName + '.' + method.Name + "\"");
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
                                        Changed = true;
                                    }
                                    break;
                                case DialogResult.No:
                                    continue;
                                default:
                                    return;
                            }
                        }
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
