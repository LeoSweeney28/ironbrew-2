using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpVarArg : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.VarArg && instruction.B != 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			"for Idx=A,B do Stk[Idx]=Vararg[Idx-A];end;";

		public override void Mutate(Instruction instruction)
		{
			instruction.B += instruction.A - 1;
		}
	}
	
	public class OpVarArgB0 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.VarArg && instruction.B == 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			"Top=A+Varargsz-1;for Idx=A,Top do local VA=Vararg[Idx-A];Stk[Idx]=VA;end;";
	}
}
