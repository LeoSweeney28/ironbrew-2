using System;
using System.Collections.Generic;
using System.Linq;
using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;
using IronBrew2.Extensions;
using IronBrew2.Utilities;

namespace IronBrew2.Obfuscator.Control_Flow.Types
{
	public static class TestPreserve
	{
		private static Random _r = SharedRandom.Instance;

		public static void DoInstructions(Chunk chunk, List<Instruction> instructions)
		{
			chunk.UpdateMappings();

			for (int idx = 0; idx < instructions.Count; idx++)
			{
				Instruction i = instructions[idx];
				switch (i.OpCode)
				{
					case Opcode.Lt:
					case Opcode.Le:
					case Opcode.Eq:
					{
						// Save comparison operands into preserved registers before
						// the comparison instruction can modify them (Lua VM can
						// reuse register slots for other values). This forces
						// deobfuscators to track register liveness.
						int mReg1 = _r.Next(200, 255);
						int mReg2 = _r.Next(200, 255);
						while (mReg2 == mReg1)
							mReg2 = _r.Next(200, 255);

						Instruction ma, mb;

						if (i.RefOperands[0] is Constant c1)
						{
							ma = new Instruction(chunk, Opcode.LoadConst, c1);
							ma.A = mReg1;
						}
						else
						{
							ma = new Instruction(chunk, Opcode.Move);
							ma.A = mReg1;
							ma.B = i.B;
						}

						if (i.RefOperands[1] is Constant c2)
						{
							mb = new Instruction(chunk, Opcode.LoadConst, c2);
							mb.A = mReg2;
						}
						else
						{
							mb = new Instruction(chunk, Opcode.Move);
							mb.A = mReg2;
							mb.B = i.C;
						}

						// Insert preservation moves before the comparison
						int insIdx = chunk.InstructionMap[i];
						chunk.Instructions.InsertRange(insIdx, new[] { ma, mb });
						chunk.UpdateMappings();

						// Update the comparison to use the preserved registers
						i.B = mReg1;
						i.C = mReg2;
						i.SetupRefs();

						// Redirect back-references to the preservation moves
						foreach (Instruction ins in i.BackReferences)
							ins.RefOperands[0] = ma;

						chunk.UpdateMappings();
						break;
					}

					case Opcode.Test:
					case Opcode.TestSet:
					{
						int rReg = _r.Next(0, 128);
						int pReg = _r.Next(250, 500);

						// Save and restore the test register around the Test instruction
						Instruction save = new Instruction(chunk, Opcode.Move);
						save.A = pReg;
						save.B = i.A;

						Instruction restore = new Instruction(chunk, Opcode.Move);
						restore.A = i.A;
						restore.B = pReg;

						// Insert save before, restore after
						int insIdx = chunk.InstructionMap[i];
						int targetIdx = insIdx + 2; // after Test + JMP

						chunk.Instructions.Insert(insIdx, save);

						if (targetIdx < chunk.Instructions.Count)
							chunk.Instructions.Insert(targetIdx + 1, restore);
						else
							chunk.Instructions.Add(restore);

						chunk.UpdateMappings();
						break;
					}
				}
			}

			chunk.UpdateMappings();
		}
	}
}