using System;
using System.Collections.Generic;
using System.Linq;
using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;
using IronBrew2.Utilities;

namespace IronBrew2.Obfuscator.Control_Flow.Types
{
    public static class EQMutate
    {
        public static Random      Random      = SharedRandom.Instance;
        public static CFGenerator CFGenerator = new CFGenerator();

        public static void DoInstructions(Chunk chunk, List<Instruction> instructions)
        {
            chunk.UpdateMappings();

            // Work on a snapshot of instructions with Eq opcodes
            var eqInstructions = instructions.Where(i => i.OpCode == Opcode.Eq).ToList();

            foreach (Instruction eq in eqInstructions)
            {
                int idx = chunk.InstructionMap[eq];
                if (idx + 1 >= chunk.Instructions.Count)
                    continue;

                Instruction jmp = chunk.Instructions[idx + 1];
                if (jmp.OpCode != Opcode.Jmp)
                    continue;

                Instruction target = (Instruction)jmp.RefOperands[0];
                Instruction fallthrough = (idx + 2 < chunk.Instructions.Count)
                    ? chunk.Instructions[idx + 2]
                    : null;

                if (fallthrough == null)
                    continue;

                // Eq A, B, C with following JMP means:
                //   if (B == C) == (A ~= 0) then JMP to target
                // Transform into equivalent Lt + Le sequence:
                //   Lt(not A) B, C -> JMP to fallthrough (skip if condition false)
                //   Le(A) B, C -> JMP to target (go to target if condition holds)
                //   JMP to target (unreachable cleanup)
                //
                // Why this works:
                //   Original: if (B == C) == not A then goto target
                //   After: if (B < C) == A then goto fallthrough (skip check)
                //          if (B <= C) == not A then goto target
                //          goto target (always)
                //   B == C is equivalent to: not (B < C) and (B <= C)
                //   So the first Lt skips when B < C (Eq would be false)
                //   The second Le goes to target when B <= C (correct when B == C)
                //   The final JMP handles the B < C case

                Instruction newLt = new Instruction(eq);
                newLt.OpCode = Opcode.Lt;
                newLt.A = eq.A == 0 ? 1 : 0;

                Instruction newLe = new Instruction(eq);
                newLe.OpCode = Opcode.Le;
                newLe.A = eq.A;

                Instruction j1 = CFGenerator.NextJMP(chunk, fallthrough);
                Instruction j2 = CFGenerator.NextJMP(chunk, target);
                Instruction j3 = CFGenerator.NextJMP(chunk, target);

                // Replace the Eq+JMP with the new sequence
                chunk.Instructions.RemoveRange(idx, 2);
                chunk.Instructions.InsertRange(idx, new[] { newLt, j1, newLe, j2, j3 });
                chunk.UpdateMappings();

                // Redirect all back-references from Eq to newLt
                foreach (Instruction br in eq.BackReferences)
                    br.RefOperands[0] = newLt;

                foreach (Instruction br in jmp.BackReferences)
                    br.RefOperands[0] = newLt;

                chunk.UpdateMappings();
            }
        }
    }
}