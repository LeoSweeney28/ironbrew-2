using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpGetGlobal : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.GetGlobal;

		public override string GetObfuscated(ObfuscationContext context) =>
			"Stk[_REG_A]=Env[_REG_B];";

		public override void Mutate(Instruction instruction)
		{
			instruction.B++;
			instruction.ConstantMask |= InstructionConstantMask.RB;
		}
	}
}
