using System;
using System.Collections.Generic;

namespace IronBrew2.Obfuscator
{
	public class ObfuscationSettings
	{
		public bool EncryptStrings { get; set; }
		public bool EncryptImportantStrings { get; set; }
		public bool ControlFlow { get; set; }
		public bool BytecodeCompress { get; set; }
		public int DecryptTableLen { get; set; }
		public bool PreserveLineInfo { get; set; }
		public bool Mutate { get; set; }
		public bool SuperOperators { get; set; }
		public int MaxMiniSuperOperators { get; set; }
		public int MaxMegaSuperOperators { get; set; }
		public int MaxMutations { get; set; }
		// When true: bytecode is AES-256-CBC encrypted instead of XOR;
		// an HMAC-SHA256 checksum is embedded and verified at VM startup.
		public bool UseAesEncryption { get; set; }

		public ObfuscationSettings()
		{
			EncryptStrings = true;
			EncryptImportantStrings = true;
			ControlFlow = true;
			BytecodeCompress = true;
			DecryptTableLen = 500;
			PreserveLineInfo = false;
			Mutate = true;
			SuperOperators = true;
			MaxMegaSuperOperators = 120;
			MaxMiniSuperOperators = 120;
			MaxMutations = 200;
			UseAesEncryption = true;
		}

		public void Validate()
		{
			List<string> errors = new List<string>();

			if (DecryptTableLen < 10 || DecryptTableLen > 10000)
				errors.Add($"DecryptTableLen must be between 10 and 10000, got {DecryptTableLen}");

			if (MaxMiniSuperOperators < 0 || MaxMiniSuperOperators > 500)
				errors.Add($"MaxMiniSuperOperators must be between 0 and 500, got {MaxMiniSuperOperators}");

			if (MaxMegaSuperOperators < 0 || MaxMegaSuperOperators > 500)
				errors.Add($"MaxMegaSuperOperators must be between 0 and 500, got {MaxMegaSuperOperators}");

			if (MaxMutations < 0 || MaxMutations > 500)
				errors.Add($"MaxMutations must be between 0 and 500, got {MaxMutations}");

			if (errors.Count > 0)
				throw new ArgumentException($"Invalid settings:\n" + string.Join("\n", errors));
		}

		public override string ToString()
		{
			return $@"
=== Obfuscation Settings ===
String Encryption:        {EncryptStrings}
Important String Encrypt: {EncryptImportantStrings}
Control Flow:             {ControlFlow}
Bytecode Compression:     {BytecodeCompress}
Decrypt Table Length:     {DecryptTableLen}
Preserve Line Info:       {PreserveLineInfo}
Mutations Enabled:        {Mutate}
Super Operators:          {SuperOperators}
Max Mini Super Ops:       {MaxMiniSuperOperators}
Max Mega Super Ops:       {MaxMegaSuperOperators}
Max Mutations:            {MaxMutations}
==========================";
		}
	}
}