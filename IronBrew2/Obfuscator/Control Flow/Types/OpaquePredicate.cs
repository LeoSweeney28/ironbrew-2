using System;
using System.Collections.Generic;
using System.Linq;
using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;
using IronBrew2.Utilities;

namespace IronBrew2.Obfuscator.Control_Flow.Types
{
    public static class OpaquePredicate
    {
        private static Random _r = SharedRandom.Instance;
        private static CFGenerator _cg = new CFGenerator();

        public static void DoInstructions(Chunk chunk, List<Instruction> instructions)
        {
            if (instructions.Count < 8)
                return;

            chunk.UpdateMappings();

            int splitCount = Math.Max(1, instructions.Count / 20);

            for (int s = 0; s < splitCount; s++)
            {
                int idx = _r.Next(2, instructions.Count - 3);

                Instruction splitIns = chunk.Instructions[idx];

                // Skip if this is a jump target
                if (splitIns.BackReferences.Count > 0 || splitIns.OpCode == Opcode.Jmp)
                    continue;

                // Save the reference to the rest of the code
                int opCount = _r.Next(2, 5);

                // Create a hash-based opaque predicate using arithmetic that always
                // resolves to a known value. Lua numbers are doubles, so we use
                // the identity: (X * 0 == 0) which is always true.
                int tempReg = _r.Next(128, 200);
                int constReg = _r.Next(200, 250);

                // Load a constant
                double magicConst = _r.Next(1, 10000);
                Constant c1 = _cg.GetOrAddConstant(chunk, ConstantType.Number, magicConst, out _);

                Instruction loadConst = new Instruction(chunk, Opcode.LoadConst, c1);
                loadConst.A = constReg;

                // Load bool (for the test)
                Instruction loadTrue = new Instruction(chunk, Opcode.LoadBool);
                loadTrue.A = tempReg;
                loadTrue.B = 1;
                loadTrue.C = 1; // skip next ins (always)

                // Test: if true then skip junk
                Instruction test = new Instruction(chunk, Opcode.Test);
                test.A = tempReg;
                test.C = 0; // if Stk[A] then ...

                // JMP to skip the junk (this will be patched)
                Instruction jmp = new Instruction(chunk, Opcode.Jmp);

                // Generate junk instructions
                var junk = new List<Instruction>();
                for (int j = 0; j < opCount; j++)
                {
                    Instruction ji = _cg.BelievableRandom(chunk);
                    ji.A = _r.Next(0, 128);
                    if (ji.B < 255) ji.B = _r.Next(0, 128);
                    if (ji.C < 255) ji.C = _r.Next(0, 128);
                    junk.Add(ji);
                }

                // Insert: loadConst, loadTrue, test, jmp, [junk...], rest of code
                int insertAt = idx;
                var toInsert = new List<Instruction> { loadConst, loadTrue, test, jmp };
                toInsert.AddRange(junk);

                chunk.Instructions.InsertRange(insertAt, toInsert);
                chunk.UpdateMappings();

                // Patch the JMP to skip past the junk
                int jmpIdx = chunk.InstructionMap[jmp];
                Instruction afterJunk = chunk.Instructions[jmpIdx + 1 + opCount];
                jmp.RefOperands[0] = afterJunk;
                jmp.SetupRefs();

                chunk.UpdateMappings();
            }
        }
    }
}
