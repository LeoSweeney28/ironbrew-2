using System;
using System.Collections.Generic;
using System.Linq;
using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;
using IronBrew2.Utilities;

namespace IronBrew2.Obfuscator.Control_Flow.Types
{
    public static class BlockSplit
    {
        private static Random _r = SharedRandom.Instance;
        private static CFGenerator _cg = new CFGenerator();

        public static void DoInstructions(Chunk chunk, List<Instruction> instructions)
        {
            if (instructions.Count < 6)
                return;

            chunk.UpdateMappings();

            int splits = Math.Max(1, instructions.Count / 15);

            for (int s = 0; s < splits; s++)
            {
                int idx = _r.Next(1, instructions.Count - 2);

                Instruction split = chunk.Instructions[idx];

                // Don't split at jump targets or jumps
                if (split.BackReferences.Count > 0)
                    continue;

                switch (split.OpCode)
                {
                    case Opcode.Jmp:
                    case Opcode.ForLoop:
                    case Opcode.ForPrep:
                    case Opcode.TForLoop:
                    case Opcode.Closure:
                    case Opcode.Return:
                    case Opcode.Close:
                    case Opcode.Eq:
                    case Opcode.Lt:
                    case Opcode.Le:
                    case Opcode.Test:
                    case Opcode.TestSet:
                    case Opcode.SetList:
                        continue;
                }

                // Split the block by inserting a JMP to the next instruction
                // This creates a new basic block boundary
                Instruction nextIns = (idx + 1 < chunk.Instructions.Count)
                    ? chunk.Instructions[idx + 1]
                    : null;

                if (nextIns == null)
                    continue;

                Instruction jmp = new Instruction(chunk, Opcode.Jmp, nextIns);
                chunk.Instructions.Insert(idx + 1, jmp);
                chunk.UpdateMappings();

                // Optionally insert a NOP-like filler at the split point
                if (_r.Next(2) == 0)
                {
                    Instruction filler = new Instruction(chunk, Opcode.Move);
                    filler.A = _r.Next(0, 128);
                    filler.B = _r.Next(0, 128);
                    chunk.Instructions.Insert(idx, filler);
                    chunk.UpdateMappings();
                }
            }
        }
    }
}
