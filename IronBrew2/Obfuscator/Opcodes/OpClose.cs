using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpClose : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Close;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local Cls={};local NLupvals={};for Idx=1,#Lupvals do local List=Lupvals[Idx];local Open=false;for Idz=0,#List do local Upv=List[Idz];local NStk=Upv[1];local Pos=Upv[2]; if NStk==Stk then if Pos>=A then Cls[Pos]=NStk[Pos];Upv[1]=Cls;else Open=true;end;end;end;if Open then NLupvals[#NLupvals+1]=List;end;end;Lupvals=NLupvals;";
	}
}
