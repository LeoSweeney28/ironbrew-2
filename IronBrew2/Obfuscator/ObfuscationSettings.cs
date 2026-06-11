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
		// When true: bytecode is AES-encrypted instead of XOR, with a checksum
		// embedded and verified at VM startup.
		// NOTE: the embedded Lua AES decryptor (VMStrings.AesDecryptorPreamble)
		// is written using Lua 5.3 bitwise operators (`~`, `//`), which are not
		// valid in the Lua 5.1 dialect this obfuscator targets (luac/luajit).
		// Enabling this currently produces output that fails to parse. Leave
		// disabled until the embedded AES implementation is rewritten using
		// Lua 5.1-compatible bit operations.
		public bool UseAesEncryption { get; set; }

		// Whether to prepend the IronBrew ASCII-art watermark/banner comment to
		// the output file. Adds several KB to the output with no functional
		// effect; disable for size-sensitive use cases.
		public bool Watermark { get; set; }

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
			UseAesEncryption = false;
			Watermark = true;
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
Watermark:                {Watermark}
==========================";
		}
	}
}