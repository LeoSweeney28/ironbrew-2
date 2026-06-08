using System;
using System.Collections.Generic;
using IronBrew2.Bytecode_Library.IR;
using IronBrew2.Cryptography;

namespace IronBrew2.Obfuscator.Encryption
{
	/// <summary>
	/// Implements integrity checking for obfuscated bytecode to prevent tampering
	/// </summary>
	public class VMIntegrityCheck
	{
		/// <summary>
		/// Computes a checksum of the instruction stream to detect modifications
		/// </summary>
		public static byte[] ComputeInstructionChecksum(List<Instruction> instructions, byte[] key)
		{
			if (instructions == null)
				throw new ArgumentNullException(nameof(instructions));
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			byte[] checksumData = new byte[instructions.Count * 8];
			int offset = 0;

			foreach (Instruction instr in instructions)
			{
				byte[] instrBytes = new byte[8];
				instrBytes[0] = (byte)instr.OpCode;
				instrBytes[1] = (byte)instr.A;
				instrBytes[2] = (byte)(instr.A >> 8);
				instrBytes[3] = (byte)instr.B;
				instrBytes[4] = (byte)(instr.B >> 8);
				instrBytes[5] = (byte)instr.C;
				instrBytes[6] = (byte)(instr.C >> 8);
				instrBytes[7] = (byte)instr.InstructionType;

				Buffer.BlockCopy(instrBytes, 0, checksumData, offset, 8);
				offset += 8;
			}

			return IntegrityProtection.CreateSignature(checksumData, key);
		}

		/// <summary>
		/// Verifies instruction stream integrity
		/// </summary>
		public static bool VerifyInstructionChecksum(List<Instruction> instructions, byte[] storedChecksum, byte[] key)
		{
			if (instructions == null)
				throw new ArgumentNullException(nameof(instructions));
			if (storedChecksum == null)
				throw new ArgumentNullException(nameof(storedChecksum));
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			byte[] computedChecksum = ComputeInstructionChecksum(instructions, key);
			return ConstantTimeComparison(storedChecksum, computedChecksum);
		}

		/// <summary>
		/// Generates VM integrity code that checks bytecode at runtime
		/// </summary>
		public static string GenerateIntegrityCheckCode(string checksumHex, ObfuscationContext context)
		{
			if (string.IsNullOrEmpty(checksumHex))
				throw new ArgumentNullException(nameof(checksumHex));
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			// Generate Lua code that verifies bytecode hasn't been modified
			string code = $@"
	local function VerifyIntegrity()
		local ChecksumHex = ""{checksumHex}"";
		local CurrentChecksum = {{}}

		for i = 1, #Instr do
			local instr = Instr[i]
			if instr then
				table.insert(CurrentChecksum, string.format(""%02x"", (instr[OP_ENUM] or 0) % 256))
			end
		end

		local ComputedHex = table.concat(CurrentChecksum, """")
		if ComputedHex ~= ChecksumHex then
			error(""INTEGRITY CHECK FAILED: Bytecode has been tampered!"")
		end
	end
	VerifyIntegrity();
";
			return code;
		}

		/// <summary>
		/// Constant-time comparison to prevent timing attacks
		/// </summary>
		private static bool ConstantTimeComparison(byte[] a, byte[] b)
		{
			if (a.Length != b.Length)
				return false;

			int result = 0;
			for (int i = 0; i < a.Length; i++)
			{
				result |= a[i] ^ b[i];
			}

			return result == 0;
		}
	}
}