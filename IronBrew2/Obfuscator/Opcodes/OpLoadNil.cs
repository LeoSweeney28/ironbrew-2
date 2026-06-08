using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpLoadNil : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.LoadNil;
		
		public override string GetObfuscated(ObfuscationContext context) =>
			"for Idx=_REG_A,_REG_B do Stk[Idx]=nil;end;";
	}
}
