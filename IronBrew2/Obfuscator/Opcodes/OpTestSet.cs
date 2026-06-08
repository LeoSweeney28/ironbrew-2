using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpTestSet : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.TestSet && instruction.C == 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local V=Stk[C];if V then InstrPoint=InstrPoint+1;else Stk[A]=V;InstrPoint=B;end;";
		
		public override void Mutate(Instruction instruction)
		{
			instruction.C = instruction.B;
			instruction.B = instruction.PC + instruction.Chunk.Instructions[instruction.PC + 1].B + 2;
			instruction.InstructionType = InstructionType.AsBxC;
		}
	}
	
	public class OpTestSetC : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.TestSet && instruction.C != 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local V=Stk[C];if not V then InstrPoint=InstrPoint+1;else Stk[A]=V;InstrPoint=B;end;";
		
		public override void Mutate(Instruction instruction)
		{
			instruction.C = instruction.B;
			instruction.B = instruction.PC + instruction.Chunk.Instructions[instruction.PC + 1].B + 2;
			instruction.InstructionType = InstructionType.AsBxC;
		}
	}
}
