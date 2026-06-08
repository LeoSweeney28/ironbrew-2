using System;
using System.IO;
using System.Linq;
using System.Text;
using IronBrew2;
using IronBrew2.Obfuscator;
using IronBrew2.Utilities;

namespace IronBrew2_CLI
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				PrintHelp();
				return;
			}

			string inputFile = null;
			string outputFile = @"C:\Users\leoli\Documents\GitHub\ironbrew-2\FINAL_OUTPUT.lua";
			string preset = "balanced";
			bool showStats = false;
			bool validate = true;
			bool showHelp = false;

			for (int i = 0; i < args.Length; i++)
			{
				string arg = args[i].ToLower();

				if (arg == "--help" || arg == "-h")
				{
					showHelp = true;
				}
				else if (arg == "--preset" && i + 1 < args.Length)
				{
					preset = args[++i];
				}
				else if ((arg == "--output" || arg == "-o") && i + 1 < args.Length)
				{
					outputFile = args[++i];
				}
				else if (arg == "--stats" || arg == "-s")
				{
					showStats = true;
				}
				else if (arg == "--no-validate")
				{
					validate = false;
				}
				else if (arg == "--presets")
				{
					Console.WriteLine(ObfuscationPresets.ListPresets());
					return;
				}
				else if (!arg.StartsWith("--"))
				{
					inputFile = args[i];
				}
			}

			if (showHelp)
			{
				PrintHelp();
				return;
			}

			if (string.IsNullOrEmpty(inputFile))
			{
				Console.WriteLine("Error: No input file specified");
				PrintUsage();
				Environment.Exit(1);
			}

			if (!File.Exists(inputFile))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Error: Input file not found: {inputFile}");
				Console.ResetColor();
				Environment.Exit(1);
			}

			try
			{
				ObfuscationSettings settings = ObfuscationPresets.GetPreset(preset);
				settings.Validate();

				long originalSize = new FileInfo(inputFile).Length;

				Console.WriteLine("\n╔════════════════════════════════════════╗");
				Console.WriteLine("║   IronBrew2 Lua Obfuscator v2.7.0      ║");
				Console.WriteLine("╚════════════════════════════════════════╝\n");

				Console.WriteLine($"Preset:      {preset.ToUpper()}");
				Console.WriteLine($"Input:       {Path.GetFileName(inputFile)} ({FormatBytes(originalSize)})");
				Console.WriteLine($"Output:      {outputFile}");
				Console.WriteLine($"Validation:  {(validate ? "Enabled" : "Disabled")}");
				Console.WriteLine();

				if (Directory.Exists("temp"))
					Directory.Delete("temp", true);
				Directory.CreateDirectory("temp");

				var stats = new ObfuscationStatistics { OriginalFileSize = originalSize };
				var onProgress = new Action<ObfuscationProgress>(progress =>
				{
					PrintProgress(progress);
				});

				if (!IB2.Obfuscate("temp", inputFile, settings, out string err, onProgress, validate))
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"\n✗ Error: {err}");
					Console.ResetColor();
					Environment.Exit(1);
				}

				string tempOutput = Path.Combine("temp", "out.lua");
				if (File.Exists(outputFile))
					File.Delete(outputFile);

				File.Move(tempOutput, outputFile);

				long obfuscatedSize = new FileInfo(outputFile).Length;
				double compression = ((originalSize - obfuscatedSize) / (double)originalSize) * 100;

				Console.WriteLine();
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("✓ Obfuscation completed successfully!");
				Console.ResetColor();

				if (showStats)
				{
					Console.WriteLine($"\n╔════════════════════════════════════════╗");
					Console.WriteLine($"║          Obfuscation Statistics         ║");
					Console.WriteLine($"╠════════════════════════════════════════╣");
					Console.WriteLine($"║ Original Size:    {FormatBytes(originalSize),21} ║");
					Console.WriteLine($"║ Obfuscated Size:  {FormatBytes(obfuscatedSize),21} ║");
					Console.WriteLine($"║ Compression:      {compression:F2}% {(compression > 0 ? "↓" : "↑"),18} ║");
					Console.WriteLine($"╚════════════════════════════════════════╝");
				}

				Console.WriteLine($"\nOutput saved to: {Path.GetFullPath(outputFile)}\n");
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"\n✗ Fatal Error: {ex.Message}");
				Console.ResetColor();

				if (ex.InnerException != null)
					Console.WriteLine($"Details: {ex.InnerException.Message}");

				Environment.Exit(1);
			}
		}

		static void PrintProgress(ObfuscationProgress progress)
		{
			Console.Write($"\r[{progress.CurrentStage}] {progress.Message}");
			if (progress.Progress.HasValue && progress.MaxProgress.HasValue)
				Console.Write($" ({progress.Progress}/{progress.MaxProgress})");
			Console.Write(new string(' ', 20));
		}

		static string FormatBytes(long bytes)
		{
			string[] sizes = { "B", "KB", "MB", "GB" };
			double len = bytes;
			int order = 0;
			while (len >= 1024 && order < sizes.Length - 1)
			{
				order++;
				len = len / 1024;
			}
			return $"{len:F2} {sizes[order]}";
		}

		static void PrintUsage()
		{
			Console.WriteLine("\nUsage: IronBrew2-CLI <input.lua> [options]\n");
			Console.WriteLine("Options:");
			Console.WriteLine("  --preset <name>    Obfuscation preset (light, balanced, heavy)");
			Console.WriteLine("                      Default: balanced");
			Console.WriteLine("  --output <file>    Output file path");
			Console.WriteLine("                      Default: out.lua");
			Console.WriteLine("  --stats            Show compression statistics");
			Console.WriteLine("  --no-validate      Skip output validation");
			Console.WriteLine("  --presets          Show available presets");
			Console.WriteLine("  --help             Show this help message");
		}

		static void PrintHelp()
		{
			Console.WriteLine("\n╔═══════════════════════════════════════════════════════════╗");
			Console.WriteLine("║     IronBrew2 Lua Obfuscator - Command Line Interface       ║");
			Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

			Console.WriteLine("DESCRIPTION:");
			Console.WriteLine("  Obfuscates Lua 5.1 code using VM-based virtualization");
			Console.WriteLine();

			Console.WriteLine("USAGE:");
			Console.WriteLine("  IronBrew2-CLI <input.lua> [options]\n");

			Console.WriteLine("EXAMPLES:");
			Console.WriteLine("  IronBrew2-CLI script.lua");
			Console.WriteLine("  IronBrew2-CLI script.lua --preset heavy --stats");
			Console.WriteLine("  IronBrew2-CLI script.lua --output obfuscated.lua\n");

			Console.WriteLine("OPTIONS:");
			Console.WriteLine("  --preset <name>      Choose obfuscation preset");
			Console.WriteLine("                        Presets: light, balanced (default), heavy");
			Console.WriteLine();
			Console.WriteLine("  --output, -o <file>  Output file path (default: out.lua)");
			Console.WriteLine();
			Console.WriteLine("  --stats, -s          Display compression statistics");
			Console.WriteLine();
			Console.WriteLine("  --no-validate        Skip syntax validation of output");
			Console.WriteLine();
			Console.WriteLine("  --presets            Show detailed preset information");
			Console.WriteLine();
			Console.WriteLine("  --help, -h           Show this help message");
			Console.WriteLine();

			Console.WriteLine("PRESETS:");
			Console.WriteLine("  LIGHT    - Fast obfuscation, minimal performance impact");
			Console.WriteLine("  BALANCED - Recommended for most use cases (default)");
			Console.WriteLine("  HEAVY    - Maximum obfuscation, significant performance impact");
			Console.WriteLine();

			Console.WriteLine("EXIT CODES:");
			Console.WriteLine("  0  - Success");
			Console.WriteLine("  1  - Error");
			Console.WriteLine();
		}
	}
}
