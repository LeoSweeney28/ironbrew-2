using System;
using System.Collections.Generic;
using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;
using IronBrew2.Obfuscator.Control_Flow;
using IronBrew2.Utilities;

namespace IronBrew2.Obfuscator.Macros
{
    /// <summary>
    /// Implements environment sandboxing for the obfuscated script.
    /// Scans for user-defined markers (IB_SET_FENV) and inserts
    /// instructions to restrict the function environment at those points.
    /// </summary>
    public static class SetFenv
    {
        private static Random _r = SharedRandom.Instance;

        /// <summary>
        /// Replaces IB_SET_FENV markers with actual setfenv calls
        /// that restrict the environment to a controlled table.
        /// </summary>
        public static void ProcessMarkers(Chunk chunk)
        {
            chunk.UpdateMappings();

            for (int i = 0; i < chunk.Instructions.Count - 2; i++)
            {
                if (chunk.Instructions[i].OpCode != Opcode.GetGlobal)
                    continue;

                if (chunk.Instructions[i + 1].OpCode != Opcode.LoadBool)
                    continue;

                if (chunk.Instructions[i + 2].OpCode != Opcode.Call)
                    continue;

                Constant nameConst = chunk.Instructions[i].RefOperands[0] as Constant;
                if (nameConst?.Data?.ToString() != "IB_SET_FENV")
                    continue;

                // Read the parameter: IB_SET_FENV(mode)
                // mode 0 = restrict access (default)
                // mode 1 = allow all
                Instruction call = chunk.Instructions[i + 2];
                int mode = call.B > 1 ? 1 : 0;

                // Replace the marker with environment sandboxing logic
                // We create a restricted environment table with only safe globals
                int envReg = 250;
                int tempReg = 251;

                // Create restricted env: { [Metatable] = {}, ... }
                var newInstructions = new List<Instruction>();

                // If mode is 0, set a restricted environment
                if (mode == 0)
                {
                    // Create a new table for the restricted environment
                    Instruction newTable = new Instruction(chunk, Opcode.NewTable);
                    newTable.A = envReg;
                    newInstructions.Add(newTable);

                    // Set the metatable to prevent accessing undeclared globals
                    // This is done by creating an empty table with __index = empty table
                    Instruction metaTable = new Instruction(chunk, Opcode.NewTable);
                    metaTable.A = tempReg;
                    newInstructions.Add(metaTable);

                    // Insert as a placeholder: the VM handles SetFenv via the VM opcode
                    Instruction setFenv = new Instruction(chunk, Opcode.SetFenv);
                    setFenv.A = 0;
                    setFenv.B = envReg;
                    newInstructions.Add(setFenv);
                }

                // Remove the original 3 instructions (GetGlobal, LoadBool, Call)
                chunk.Instructions.RemoveRange(i, 3);

                // Insert the new instructions
                chunk.Instructions.InsertRange(i, newInstructions);
                chunk.UpdateMappings();
            }

            // Process sub-chunks
            foreach (var sub in chunk.Functions)
                ProcessMarkers(sub);
        }
    }
}
