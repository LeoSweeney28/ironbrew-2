using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IronBrew2.Bytecode_Library.IR;
using IronBrew2.Utilities;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpSuperOperator : VOpcode
	{
		public VOpcode[] SubOpcodes;

		public override bool IsInstruction(Instruction instruction) =>
			false;

		public bool IsInstruction(List<Instruction> instructions)
		{
			if (instructions.Count != SubOpcodes.Length)
				return false;

			for (int i = 0; i < SubOpcodes.Length; i++)
			{
				VOpcode op = SubOpcodes[i];

				if (op is OpMutated mut)
				{
					if (!mut.Mutated.IsInstruction(instructions[i]))
						return false;
				}
				else if (!op.IsInstruction(instructions[i]))
					return false;
			}

			return true;
		}

		public override string GetObfuscated(ObfuscationContext context)
		{
			var r = SharedRandom.Instance;
			List<string> pieces = new List<string>();
			List<string> allLocals = new List<string>();
			string uniqueId = r.Next(10000, 99999).ToString();

			for (var index = 0; index < SubOpcodes.Length; index++)
			{
				var subOpcode = SubOpcodes[index];
				string s2 = subOpcode.GetObfuscated(context);

				// Local variable hoisting - rename locals to avoid conflicts
				Regex reg = new Regex("local\\s+(\\w+)\\s*[;=]");
				foreach (Match m in reg.Matches(s2))
				{
					string loc = m.Groups[1].Value;
					string renamed = loc + "_" + uniqueId + "_" + index;
					if (!allLocals.Contains(renamed))
						allLocals.Add(renamed);

					s2 = Regex.Replace(s2, @"\b" + loc + @"\b", renamed);
				}

				pieces.Add(s2);

				if (index + 1 < SubOpcodes.Length)
				{
					pieces.Add("InstrPoint=InstrPoint+1;");
					pieces.Add("Inst=Instr[InstrPoint];");
					pieces.Add("A=Inst[2];B=Inst[3];C=Inst[4];");
				}
			}

			string result = string.Join("", pieces);

			if (allLocals.Count > 0)
				result = "local " + string.Join(",", allLocals) + ";" + result;

			return result;
		}
	}
}