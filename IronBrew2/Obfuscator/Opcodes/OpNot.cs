using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpNot : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Not;

		public override string GetObfuscated(ObfuscationContext context) =>
			"Stk[_REG_A]=(not Stk[_REG_B]);";
	}
}
