using System;
using System.Collections.Generic;

namespace IronBrew2.Obfuscator
{
	public static class ObfuscationPresets
	{
		public static ObfuscationSettings Light => new ObfuscationSettings
		{
			EncryptStrings = true,
			EncryptImportantStrings = false,
			ControlFlow = false,
			BytecodeCompress = true,
			DecryptTableLen = 100,
			PreserveLineInfo = true,
			Mutate = false,
			SuperOperators = false,
			MaxMiniSuperOperators = 20,
			MaxMegaSuperOperators = 20,
			MaxMutations = 50
		};

		public static ObfuscationSettings Balanced => new ObfuscationSettings
		{
			EncryptStrings = true,
			EncryptImportantStrings = true,
			ControlFlow = true,
			BytecodeCompress = true,
			DecryptTableLen = 500,
			PreserveLineInfo = true,
			Mutate = true,
			SuperOperators = true,
			MaxMiniSuperOperators = 120,
			MaxMegaSuperOperators = 120,
			MaxMutations = 200
		};

		public static ObfuscationSettings Heavy => new ObfuscationSettings
		{
			EncryptStrings = true,
			EncryptImportantStrings = true,
			ControlFlow = true,
			BytecodeCompress = true,
			DecryptTableLen = 1000,
			PreserveLineInfo = false,
			Mutate = true,
			SuperOperators = true,
			MaxMiniSuperOperators = 300,
			MaxMegaSuperOperators = 300,
			MaxMutations = 500
		};

		public static ObfuscationSettings GetPreset(string name)
		{
			return name?.ToLower() switch
			{
				"light" => Light,
				"balanced" or "medium" => Balanced,
				"heavy" => Heavy,
				_ => throw new ArgumentException($"Unknown preset: {name}")
			};
		}

		public static IEnumerable<(string name, ObfuscationSettings settings)> GetAllPresets()
		{
			yield return ("light", Light);
			yield return ("balanced", Balanced);
			yield return ("heavy", Heavy);
		}

		public static string ListPresets()
		{
			return @"
Available Obfuscation Presets:

  LIGHT
    - Fast obfuscation
    - Minimal performance impact
    - Basic encryption only
    - Best for: Non-critical code

  BALANCED (DEFAULT)
    - Good balance of speed and security
    - Reasonable performance impact
    - Most features enabled
    - Best for: General use

  HEAVY
    - Maximum obfuscation strength
    - Significant performance impact
    - All features enabled at maximum
    - Best for: Sensitive code
";
		}
	}
}
