using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace ContraCrack.Util
{
    public static class Cecil
    {
        //These extend AssemblyDefinition and allow me to assembly.hasStrongName() instead of Cecil.hasStrongName(assembly)
        public static bool HasStrongName(this AssemblyDefinition asm)
        {
            return asm.Name.HasPublicKey;
        }

        public static void RemoveStrongName(this AssemblyDefinition asm)
        {
            asm.Name.PublicKey = new byte[0];
            asm.Name.PublicKeyToken = new byte[0];
            asm.Name.IsSideBySideCompatible = true;
        }
        public static int RemoveInvalidOpCodes(this MethodDefinition method)
        {
            List<Instruction> InstToRemove =
                method.Body.Instructions.Where(t => t.OpCode == OpCodes.Invalid).ToList();
            foreach (Instruction t in InstToRemove)
            {
                method.Body.GetILProcessor().Remove(t);
                //method.Body.GetILProcessor().Replace(t, method.Body.GetILProcessor().Create(OpCodes.Nop));
            }
            method.Clean();
            return InstToRemove.Count;
        }
        public static MethodDefinition Clean(this MethodDefinition method)
        {
            method.Body.SimplifyMacros();
            method.Body.OptimizeMacros();
            return method;
        }
        public static MethodDefinition Clear(this MethodDefinition method)
        {
            method.Body.Instructions.Clear();
            method.Body.Variables.Clear();
            method.Body.ExceptionHandlers.Clear();
            return method;
        }
        public static MethodDefinition[] GetConstructors(this TypeDefinition type)
        {
            return type.Methods.Where(methodt => methodt.IsConstructor).ToArray();
        }
        public static MethodDefinition AppendMethod(this MethodDefinition inputMethod, MethodDefinition appendMethod)
        {

            int count = inputMethod.Body.Instructions.Count;
            if (count > 0)
            {
                inputMethod.Body.GetILProcessor().Remove(inputMethod.Body.Instructions[count - 1]);
            }

            for (int x = 0; x < appendMethod.Body.Instructions.Count; x++)
            {
                inputMethod.Body.GetILProcessor().Append(appendMethod.Body.Instructions[x]);
            }
            inputMethod.Clean();
            return inputMethod;
        }
        //This is deobfuscation stuff, I'm hoping to turn this into an invalid opcode remover/renamer too
        public static void RemoveIllegalConstruct(MethodDefinition method)
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

        public static void UpdateInstructionReferences(MethodDefinition method, Instruction oldTarget, Instruction newTarget)
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
//
// MethodBodyRocks.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2010 Jb Evain
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace Mono.Cecil.Rocks {
	public static class MethodBodyRocks {

		public static void SimplifyMacros (this MethodBody self)
		{
			if (self == null)
				throw new ArgumentNullException ("self");

			foreach (var instruction in self.Instructions) {
				if (instruction.OpCode.OpCodeType != OpCodeType.Macro)
					continue;
				switch (instruction.OpCode.Code) {
                        /*
				case Code.Ldarg_0:
					ExpandMacro (instruction, OpCodes.Ldarg, self.Method.Parameters[0]);
					break;
				case Code.Ldarg_1:
					ExpandMacro (instruction, OpCodes.Ldarg, self.Method.Parameters[1]);
					break;
				case Code.Ldarg_2:
					ExpandMacro (instruction, OpCodes.Ldarg, self.Method.Parameters[2]);
					break;
				case Code.Ldarg_3:
					ExpandMacro (instruction, OpCodes.Ldarg, self.Method.Parameters[3]);
					break;
                        */
				case Code.Ldloc_0:
					ExpandMacro (instruction, OpCodes.Ldloc, self.Variables [0]);
					break;
				case Code.Ldloc_1:
					ExpandMacro (instruction, OpCodes.Ldloc, self.Variables [1]);
					break;
				case Code.Ldloc_2:
					ExpandMacro (instruction, OpCodes.Ldloc, self.Variables [2]);
					break;
				case Code.Ldloc_3:
					ExpandMacro (instruction, OpCodes.Ldloc, self.Variables [3]);
					break;
				case Code.Stloc_0:
					ExpandMacro (instruction, OpCodes.Stloc, self.Variables [0]);
					break;
				case Code.Stloc_1:
					ExpandMacro (instruction, OpCodes.Stloc, self.Variables [1]);
					break;
				case Code.Stloc_2:
					ExpandMacro (instruction, OpCodes.Stloc, self.Variables [2]);
					break;
				case Code.Stloc_3:
					ExpandMacro (instruction, OpCodes.Stloc, self.Variables [3]);
					break;
				case Code.Ldarg_S:
					instruction.OpCode = OpCodes.Ldarg;
					break;
				case Code.Ldarga_S:
					instruction.OpCode = OpCodes.Ldarga;
					break;
				case Code.Starg_S:
					instruction.OpCode = OpCodes.Starg;
					break;
				case Code.Ldloc_S:
					instruction.OpCode = OpCodes.Ldloc;
					break;
				case Code.Ldloca_S:
					instruction.OpCode = OpCodes.Ldloca;
					break;
				case Code.Stloc_S:
					instruction.OpCode = OpCodes.Stloc;
					break;
				case Code.Ldc_I4_M1:
					ExpandMacro (instruction, OpCodes.Ldc_I4, -1);
					break;
				case Code.Ldc_I4_0:
					ExpandMacro (instruction, OpCodes.Ldc_I4, 0);
					break;
				case Code.Ldc_I4_1:
					ExpandMacro (instruction, OpCodes.Ldc_I4, 1);
					break;
				case Code.Ldc_I4_2:
					ExpandMacro (instruction, OpCodes.Ldc_I4, 2);
					break;
				case Code.Ldc_I4_3:
					ExpandMacro (instruction, OpCodes.Ldc_I4, 3);
					break;
				case Code.Ldc_I4_4:
					ExpandMacro (instruction, OpCodes.Ldc_I4, 4);
					break;
				case Code.Ldc_I4_5:
					ExpandMacro (instruction, OpCodes.Ldc_I4, 5);
					break;
				case Code.Ldc_I4_6:
					ExpandMacro (instruction, OpCodes.Ldc_I4, 6);
					break;
				case Code.Ldc_I4_7:
					ExpandMacro (instruction, OpCodes.Ldc_I4, 7);
					break;
				case Code.Ldc_I4_8:
					ExpandMacro (instruction, OpCodes.Ldc_I4, 8);
					break;
				case Code.Ldc_I4_S:
					ExpandMacro (instruction, OpCodes.Ldc_I4, (int) (sbyte) instruction.Operand);
					break;
				case Code.Br_S:
					instruction.OpCode = OpCodes.Br;
					break;
				case Code.Brfalse_S:
					instruction.OpCode = OpCodes.Brfalse;
					break;
				case Code.Brtrue_S:
					instruction.OpCode = OpCodes.Brtrue;
					break;
				case Code.Beq_S:
					instruction.OpCode = OpCodes.Beq;
					break;
				case Code.Bge_S:
					instruction.OpCode = OpCodes.Bge;
					break;
				case Code.Bgt_S:
					instruction.OpCode = OpCodes.Bgt;
					break;
				case Code.Ble_S:
					instruction.OpCode = OpCodes.Ble;
					break;
				case Code.Blt_S:
					instruction.OpCode = OpCodes.Blt;
					break;
				case Code.Bne_Un_S:
					instruction.OpCode = OpCodes.Bne_Un;
					break;
				case Code.Bge_Un_S:
					instruction.OpCode = OpCodes.Bge_Un;
					break;
				case Code.Bgt_Un_S:
					instruction.OpCode = OpCodes.Bgt_Un;
					break;
				case Code.Ble_Un_S:
					instruction.OpCode = OpCodes.Ble_Un;
					break;
				case Code.Blt_Un_S:
					instruction.OpCode = OpCodes.Blt_Un;
					break;
				case Code.Leave_S:
					instruction.OpCode = OpCodes.Leave;
					break;
				}
			}
		}

		static void ExpandMacro (Instruction instruction, OpCode opcode, object operand)
		{
			instruction.OpCode = opcode;
			instruction.Operand = operand;
		}

		static void MakeMacro (Instruction instruction, OpCode opcode)
		{
			instruction.OpCode = opcode;
			instruction.Operand = null;
		}

		public static void OptimizeMacros (this MethodBody self)
		{
			if (self == null)
				throw new ArgumentNullException ("self");

			var method = self.Method;

			foreach (var instruction in self.Instructions) {
				int index;
				switch (instruction.OpCode.Code) {
				case Code.Ldarg:
					index = ((ParameterDefinition) instruction.Operand).Index;
					if (index == -1 && instruction.Operand == self.ThisParameter)
						index = 0;
					else if (method.HasThis)
						index++;

					switch (index) {
					case 0:
						MakeMacro (instruction, OpCodes.Ldarg_0);
						break;
					case 1:
						MakeMacro (instruction, OpCodes.Ldarg_1);
						break;
					case 2:
						MakeMacro (instruction, OpCodes.Ldarg_2);
						break;
					case 3:
						MakeMacro (instruction, OpCodes.Ldarg_3);
						break;
					default:
						if (index < 256)
							ExpandMacro (instruction, OpCodes.Ldarg_S, instruction.Operand);
						break;
					}
					break;
				case Code.Ldloc:
					index = ((VariableDefinition) instruction.Operand).Index;
					switch (index) {
					case 0:
						MakeMacro (instruction, OpCodes.Ldloc_0);
						break;
					case 1:
						MakeMacro (instruction, OpCodes.Ldloc_1);
						break;
					case 2:
						MakeMacro (instruction, OpCodes.Ldloc_2);
						break;
					case 3:
						MakeMacro (instruction, OpCodes.Ldloc_3);
						break;
					default:
						if (index < 256)
							ExpandMacro (instruction, OpCodes.Ldloc_S, instruction.Operand);
						break;
					}
					break;
				case Code.Stloc:
					index = ((VariableDefinition) instruction.Operand).Index;
					switch (index) {
					case 0:
						MakeMacro (instruction, OpCodes.Stloc_0);
						break;
					case 1:
						MakeMacro (instruction, OpCodes.Stloc_1);
						break;
					case 2:
						MakeMacro (instruction, OpCodes.Stloc_2);
						break;
					case 3:
						MakeMacro (instruction, OpCodes.Stloc_3);
						break;
					default:
						if (index < 256)
							ExpandMacro (instruction, OpCodes.Stloc_S, instruction.Operand);
						break;
					}
					break;
				case Code.Ldarga:
					index = ((ParameterDefinition) instruction.Operand).Index;
					if (index == -1 && instruction.Operand == self.ThisParameter)
						index = 0;
					else if (method.HasThis)
						index++;
					if (index < 256)
						ExpandMacro (instruction, OpCodes.Ldarga_S, instruction.Operand);
					break;
				case Code.Ldloca:
					if (((VariableDefinition) instruction.Operand).Index < 256)
						ExpandMacro (instruction, OpCodes.Ldloca_S, instruction.Operand);
					break;
				case Code.Ldc_I4:
					int i = (int) instruction.Operand;
					switch (i) {
					case -1:
						MakeMacro (instruction, OpCodes.Ldc_I4_M1);
						break;
					case 0:
						MakeMacro (instruction, OpCodes.Ldc_I4_0);
						break;
					case 1:
						MakeMacro (instruction, OpCodes.Ldc_I4_1);
						break;
					case 2:
						MakeMacro (instruction, OpCodes.Ldc_I4_2);
						break;
					case 3:
						MakeMacro (instruction, OpCodes.Ldc_I4_3);
						break;
					case 4:
						MakeMacro (instruction, OpCodes.Ldc_I4_4);
						break;
					case 5:
						MakeMacro (instruction, OpCodes.Ldc_I4_5);
						break;
					case 6:
						MakeMacro (instruction, OpCodes.Ldc_I4_6);
						break;
					case 7:
						MakeMacro (instruction, OpCodes.Ldc_I4_7);
						break;
					case 8:
						MakeMacro (instruction, OpCodes.Ldc_I4_8);
						break;
					default:
						if (i >= -128 && i < 128)
							ExpandMacro (instruction, OpCodes.Ldc_I4_S, (sbyte) i);
						break;
					}
					break;
				}
			}

			OptimizeBranches (self);
		}

		static void OptimizeBranches (MethodBody body)
		{
			ComputeOffsets (body);

			foreach (var instruction in body.Instructions) {
				if (instruction.OpCode.OperandType != OperandType.InlineBrTarget)
					continue;

				if (OptimizeBranch (instruction))
					ComputeOffsets (body);
			}
		}

		static bool OptimizeBranch (Instruction instruction)
		{
			var offset = ((Instruction) instruction.Operand).Offset - (instruction.Offset + instruction.OpCode.Size + 4);
			if (!(offset >= -128 && offset <= 127))
				return false;

			switch (instruction.OpCode.Code) {
			case Code.Br:
				instruction.OpCode = OpCodes.Br_S;
				break;
			case Code.Brfalse:
				instruction.OpCode = OpCodes.Brfalse_S;
				break;
			case Code.Brtrue:
				instruction.OpCode = OpCodes.Brtrue_S;
				break;
			case Code.Beq:
				instruction.OpCode = OpCodes.Beq_S;
				break;
			case Code.Bge:
				instruction.OpCode = OpCodes.Bge_S;
				break;
			case Code.Bgt:
				instruction.OpCode = OpCodes.Bgt_S;
				break;
			case Code.Ble:
				instruction.OpCode = OpCodes.Ble_S;
				break;
			case Code.Blt:
				instruction.OpCode = OpCodes.Blt_S;
				break;
			case Code.Bne_Un:
				instruction.OpCode = OpCodes.Bne_Un_S;
				break;
			case Code.Bge_Un:
				instruction.OpCode = OpCodes.Bge_Un_S;
				break;
			case Code.Bgt_Un:
				instruction.OpCode = OpCodes.Bgt_Un_S;
				break;
			case Code.Ble_Un:
				instruction.OpCode = OpCodes.Ble_Un_S;
				break;
			case Code.Blt_Un:
				instruction.OpCode = OpCodes.Blt_Un_S;
				break;
			case Code.Leave:
				instruction.OpCode = OpCodes.Leave_S;
				break;
			}

			return true;
		}

		static void ComputeOffsets (MethodBody body)
		{
			var offset = 0;
			foreach (var instruction in body.Instructions) {
				instruction.Offset = offset;
				offset += instruction.GetSize ();
			}
		}
	}
}
