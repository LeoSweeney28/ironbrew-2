using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpGetTable : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.GetTable && instruction.C <= 255;

		public override string GetObfuscated(ObfuscationContext context) =>
			"Stk[_REG_A]=Stk[_REG_B][Stk[_REG_C]];";
	}
	
	public class OpGetTableConst : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.GetTable && instruction.C > 255;

		public override string GetObfuscated(ObfuscationContext context) =>
			"Stk[_REG_A]=Stk[_REG_B][_REG_C];";

		public override void Mutate(Instruction instruction)
		{
			instruction.C -= 255;
			instruction.ConstantMask |= InstructionConstantMask.RC;
		}
	}
}
