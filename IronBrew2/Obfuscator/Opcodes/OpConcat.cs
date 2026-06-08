using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpConcat : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Concat;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local V=B;local K=Stk[V] for Idx=V+1,C do K=K..Stk[Idx];end;Stk[A]=K;";
	}
}
