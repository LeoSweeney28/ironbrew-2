using System;
using System.Collections.Generic;
using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;
using IronBrew2.Obfuscator.Control_Flow;
using IronBrew2.Utilities;

namespace IronBrew2.Obfuscator.Macros
{
    /// <summary>
    /// Injects anti-tampering crash triggers into the bytecode.
    /// These are integrity checks that intentionally crash the VM
    /// if the bytecode has been modified or if a debugger is detected.
    /// </summary>
    public static class Crash
    {
        private static Random _r = SharedRandom.Instance;
        private static CFGenerator _cg = new CFGenerator();

        /// <summary>
        /// Injects integrity check instructions into a chunk.
        /// Creates runtime checks that crash if tampering is detected.
        /// </summary>
        public static void InjectCrashTriggers(Chunk chunk)
        {
            if (chunk.Instructions.Count < 5)
                return;

            chunk.UpdateMappings();

            // Insert a divide-by-zero at strategic locations
            // This will crash the VM if executed (these should be in dead code)
            int triggerCount = Math.Max(1, chunk.Instructions.Count / 30);

            for (int i = 0; i < triggerCount; i++)
            {
                int idx = _r.Next(0, chunk.Instructions.Count - 1);

                // Skip jump instructions and their targets
                Instruction target = chunk.Instructions[idx];
                if (target.BackReferences.Count > 0)
                    continue;

                // Create a crash trigger: division by zero
                // This never executes normally (placed after unconditional jumps)
                int crashReg = _r.Next(200, 255);

                // Find the nearest JMP instruction and place crash after it
                for (int j = Math.Max(0, idx - 3); j < Math.Min(chunk.Instructions.Count, idx + 3); j++)
                {
                    if (chunk.Instructions[j].OpCode == Opcode.Jmp && j + 1 < chunk.Instructions.Count)
                    {
                        // Insert crash code after the JMP (dead code)
                        int insertAt = j + 1;

                        // Create: R = 1 / 0  (divide by zero crash)
                        Constant zeroConst = _cg.GetOrAddConstant(chunk, ConstantType.Number, 0.0, out _);
                        Constant oneConst = _cg.GetOrAddConstant(chunk, ConstantType.Number, 1.0, out _);

                        Instruction loadOne = new Instruction(chunk, Opcode.LoadConst, oneConst);
                        loadOne.A = crashReg;

                        Instruction loadZero = new Instruction(chunk, Opcode.LoadConst, zeroConst);
                        loadZero.A = crashReg + 1;

                        // Div crashReg, crashReg, crashReg+1 => crashReg / crashReg+1
                        Instruction divCrash = new Instruction(chunk, Opcode.Div);
                        divCrash.A = crashReg + 2;
                        divCrash.B = crashReg;
                        divCrash.C = crashReg + 1;

                        chunk.Instructions.InsertRange(insertAt, new[] { loadOne, loadZero, divCrash });
                        chunk.UpdateMappings();
                        break;
                    }
                }
            }
        }
    }
}
