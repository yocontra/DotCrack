using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;

namespace ContraCrack.Util
{
    public static class Cecil
    {
        //These extend AssemblyDefinition and allow me to assembly.hasStrongName() instead of Cecil.hasStrongName(assembly)
        public static bool hasStrongName(this AssemblyDefinition asm)
        {
            if (asm.Name.HasPublicKey)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void removeStrongName(this AssemblyDefinition asm)
        {
            asm.Name.PublicKey = new byte[0];
            asm.Name.PublicKeyToken = new byte[0];
            asm.Name.Flags = AssemblyFlags.SideBySideCompatible;
        }
        public static MethodDefinition appendMethod(this MethodDefinition inputMethod, MethodDefinition appendMethod)
        {
            int count = inputMethod.Body.Instructions.Count;
            if (count > 0)
            {
                inputMethod.Body.CilWorker.Remove(inputMethod.Body.Instructions[count - 1]);
            }
            for (int x = 0; x < appendMethod.Body.Instructions.Count; x++)
            {
                inputMethod.Body.CilWorker.Append(appendMethod.Body.Instructions[x]);
            }
            return inputMethod;
        }
        public static MethodReference getGenericMethodRef(MethodReference method, TypeReference declaringType)
        {
            var reference = new MethodReference(method.Name, declaringType, method.ReturnType.ReturnType, method.HasThis, method.ExplicitThis, MethodCallingConvention.Generic);
            foreach (ParameterDefinition parameter in method.Parameters)
            {
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
            }
            return reference;
        }
        //This is deobfuscation stuff, I'm hoping to turn this into an invalid opcode remover/renamer too
        public static void RemoveIllegalConstruct(Mono.Cecil.MethodDefinition method)
        {
            if (!method.HasBody)
                return;
            for (int i = 0; i < method.Body.Instructions.Count - 5; i++)
            {
                if (method.Body.Instructions[i].OpCode.Code == Code.Ldc_I4_1 &&
                    method.Body.Instructions[i + 1].OpCode.FlowControl == FlowControl.Branch &&
                    method.Body.Instructions[i + 1].Operand == method.Body.Instructions[i + 4] &&
                    method.Body.Instructions[i + 2].OpCode.Code == Code.Ldc_I4_0 &&
                    method.Body.Instructions[i + 3].OpCode.FlowControl == FlowControl.Branch &&
                    method.Body.Instructions[i + 3].Operand == method.Body.Instructions[i + 4] &&
                    method.Body.Instructions[i + 4].OpCode.Code == Code.Brfalse_S)
                {
                    UpdateInstructionReferences(method, method.Body.Instructions[i], method.Body.Instructions[i + 5]);
                    for (int j = 0; j < 5; j++)
                        method.Body.Instructions.RemoveAt(i);
                    i--;
                }
            }
        }

        public static void UpdateInstructionReferences(Mono.Cecil.MethodDefinition method, Instruction oldTarget, Instruction newTarget)
        {
            for (int j = 0; j < method.Body.Instructions.Count; j++)
            {
                if ((method.Body.Instructions[j].OpCode.FlowControl == FlowControl.Branch ||
                    method.Body.Instructions[j].OpCode.FlowControl == FlowControl.Cond_Branch) &&
                    method.Body.Instructions[j].Operand == oldTarget)
                    method.Body.Instructions[j].Operand = newTarget;
            }
            foreach (ExceptionHandler v in method.Body.ExceptionHandlers)
            {
                if (v.FilterEnd == oldTarget)
                    v.FilterEnd = newTarget;
                if (v.FilterStart == oldTarget)
                    v.FilterStart = newTarget;
                if (v.HandlerEnd == oldTarget)
                    v.HandlerEnd = newTarget;
                if (v.HandlerStart == oldTarget)
                    v.HandlerStart = newTarget;
                if (v.TryEnd == oldTarget)
                    v.TryEnd = newTarget;
                if (v.TryStart == oldTarget)
                    v.TryStart = newTarget;
            }
        }
    }
}
