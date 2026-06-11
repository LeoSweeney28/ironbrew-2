using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;
using IronBrew2.Utilities;

namespace IronBrew2.Obfuscator.Control_Flow.Types
{
	public static class Bounce
	{
		public static Random      Random      = SharedRandom.Instance;
		public static CFGenerator CFGenerator = new CFGenerator();

		public static void DoInstructions(Chunk chunk, List<Instruction> Instructions)
		{
			Instructions = Instructions.ToList();
			foreach (Instruction l in Instructions)
			{
				if (l.OpCode != Opcode.Jmp)
					continue;

				Instruction First = CFGenerator.NextJMP(chunk, (Instruction) l.RefOperands[0]);
				chunk.Instructions.Add(First);		
				l.RefOperands[0] = First;
			}
			
			chunk.UpdateMappings();
		}
	}
}