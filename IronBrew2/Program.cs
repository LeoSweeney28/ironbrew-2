using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;
using IronBrew2.Cryptography;
using IronBrew2.Obfuscator;
using IronBrew2.Obfuscator.Control_Flow;
using IronBrew2.Obfuscator.Control_Flow.Types;
using IronBrew2.Obfuscator.Encryption;
using IronBrew2.Obfuscator.Macros;
using IronBrew2.Obfuscator.VM_Generation;
using IronBrew2.Utilities;
using IronBrew2.Validation;

namespace IronBrew2
{
	public static class IB2
	{
		private static Encoding LuaBytecodeEncoding => EncodingConstants.LuaBytecodeEncoding;

		public static bool Obfuscate(string path, string input, ObfuscationSettings settings, out string error,
		Action<ObfuscationProgress> onProgress = null, bool validate = true)
		{
			try
			{
				error = "";
				settings.Validate();
				var metrics = new PipelineMetrics();
				metrics.StartTotal();

				onProgress?.Invoke(new ObfuscationProgress(ObfuscationStage.SyntaxCheck, "Starting obfuscation..."));

				// On Windows, Process.Start (UseShellExecute=false) does not search the
				// current directory for relative file names, so the bundled luac/luajit
				// binaries (which live next to this assembly) must be referenced by their
				// full path. On Unix-likes we fall back to the system-installed tools.
				bool isUnix = Environment.OSVersion.Platform == PlatformID.Unix;
				string OS = isUnix ? "/usr/bin/" : AppContext.BaseDirectory;
				string EXE = isUnix ? "" : ".exe";

				string l = Path.Combine(path, "luac.out");

				if (!File.Exists(input))
					throw new Exception("Invalid input file.");

				Console.WriteLine("Checking file...");

				Process proc = new Process
				       {
					       StartInfo =
					       {
						       FileName  = $"{OS}luac{EXE}",
						       Arguments = "-o \"" + l + "\" \"" + input + "\"",
						       UseShellExecute = false,
						       RedirectStandardError = true,
						       RedirectStandardOutput = true
					       }
				       };

				string err = "";

				proc.OutputDataReceived += (sender, args) => { if (args.Data != null) err += args.Data + "\n"; };
				proc.ErrorDataReceived += (sender, args) => { if (args.Data != null) err += args.Data + "\n"; };

				proc.Start();
				proc.BeginOutputReadLine();
				proc.BeginErrorReadLine();
				proc.WaitForExit();

				error = err;

				if (!File.Exists(l))
					return false;

				File.Delete(l);
				string t0 = Path.Combine(path, "t0.lua");

				Console.WriteLine("Stripping comments...");

				err = "";
				proc = new Process
				       {
					       StartInfo =
					       {
						       FileName = $"{OS}luajit{EXE}",
						       Arguments =
							       "../Lua/Minifier/luasrcdiet.lua --noopt-whitespace --noopt-emptylines --noopt-numbers --noopt-locals --noopt-strings --opt-comments \"" +
							       input                                                       +
							       "\" -o \""                                                  + t0 + "\"",
						       UseShellExecute        = false,
						       RedirectStandardError  = true,
						       RedirectStandardOutput = true
					       }
				       };

				proc.OutputDataReceived += (sender, args) => { if (args.Data != null) err += args.Data + "\n"; };
				proc.ErrorDataReceived  += (sender, args) => { if (args.Data != null) err += args.Data + "\n"; };

				proc.Start();
				proc.BeginOutputReadLine();
				proc.BeginErrorReadLine();
				proc.WaitForExit();

				error = err;

				if (!File.Exists(t0))
					return false;

				string t1 = Path.Combine(path, "t1.lua");

				Console.WriteLine("Compiling...");

				File.WriteAllText(t1, new ConstantEncryption(settings, File.ReadAllText(t0, LuaBytecodeEncoding)).EncryptStrings());
				err = "";
				proc = new Process
				       {
					       StartInfo =
					       {
						       FileName  = $"{OS}luac{EXE}",
						       Arguments = "-o \"" + l + "\" \"" + t1 + "\"",
						       UseShellExecute = false,
						       RedirectStandardError = true,
						       RedirectStandardOutput = true
					       }
				       };

				proc.OutputDataReceived += (sender, args) => { if (args.Data != null) err += args.Data + "\n"; };
				proc.ErrorDataReceived += (sender, args) => { if (args.Data != null) err += args.Data + "\n"; };

				proc.Start();
				proc.BeginOutputReadLine();
				proc.BeginErrorReadLine();
				proc.WaitForExit();

				error = err;

				if (!File.Exists(l))
					return false;

				Console.WriteLine("Obfuscating...");

				Deserializer des    = new Deserializer(File.ReadAllBytes(l));
				Chunk lChunk = des.DecodeFile();

				// ConstantEncryption can wrap encrypted string literals in an IIFE that
				// starts with calls to the IB_INLINING_START marker function - via the
				// EncryptImportantStrings pass and the [STR_ENCRYPT]-tag pass (which run
				// regardless of EncryptStrings), not just the general EncryptStrings pass.
				// Inlining.DoChunks strips/converts those markers (and optionally inlines
				// the IIFE), and is a cheap no-op when none are present. Run it
				// unconditionally so no IB_INLINING_START call is ever left as a reference
				// to an undefined global, which would crash the obfuscated script at runtime.
				new Inlining(lChunk).DoChunks();

				if (settings.ControlFlow)
				{
					CFContext cf = new CFContext(lChunk);
					cf.DoChunks();
				}

				Console.WriteLine("Serializing...");

				//shuffle stuff
				//lChunk.Constants.Shuffle();
				//lChunk.Functions.Shuffle();

				ObfuscationContext context = new ObfuscationContext(lChunk);

				string t2 = Path.Combine(path, "t2.lua");
				string c = new Generator(context).GenerateVM(settings);

				//string byteLocal = c.Substring(null, "\n");
				//string rest = c.Substring("\n");

				File.WriteAllText(t2, c, LuaBytecodeEncoding);

				string t3 = Path.Combine(path, "t3.lua");

				Console.WriteLine("Minifying...");

				err = "";
				proc = new Process
				       {
					       StartInfo =
					       {
						       FileName = $"{OS}luajit{EXE}",
						       Arguments =
							       "../Lua/Minifier/luasrcdiet.lua --maximum --opt-entropy --opt-emptylines --opt-eols --opt-numbers --opt-whitespace --opt-locals --noopt-strings \"" +
							       t2                                                                                                                                                +
							       "\" -o \"" +
							        t3 +
							       "\"",
						       UseShellExecute = false,
						       RedirectStandardError = true,
						       RedirectStandardOutput = true
					       }
				       };

				proc.OutputDataReceived += (sender, args) => { if (args.Data != null) err += args.Data + "\n"; };
				proc.ErrorDataReceived += (sender, args) => { if (args.Data != null) err += args.Data + "\n"; };

				proc.Start();
				proc.BeginOutputReadLine();
				proc.BeginErrorReadLine();
				proc.WaitForExit();

				error = err;

				if (!File.Exists(t3))
					return false;

				string outLua = Path.Combine(path, "out.lua");
				string minified = File.ReadAllText(t3, LuaBytecodeEncoding).Replace("\n", " ");

				if (settings.Watermark)
				{
					Console.WriteLine("Watermark...");

					File.WriteAllText(outLua, @"--[[
IronBrew:tm: obfuscation; Version 2.7.0

........................................................................................................................................................................................................
........................................................................................................................................................................................................
.....,,...,.............................................................................................................................................................................................
.... MMMMM,.............................................................................................................................................................................................
....MMMMMMM,............................................................................................................................................................................................
....MMMMMMM,............................................................................................................................................................................................
....,MMMMMO.............................................................................................................................................................................................
......,.................................................................................................................................................................................................
..................................................,,,,,,............................................Z$$.................................................................................................
...................................................:::::............................................MMMO................................................................................................
.....:???? ,.......:????....,.8MMMMM,.......,,,MMMMI???INMMM.,................,.?ZMMMMDI:,,.........MMM$................................................................................................
.....MMMMM?,.......MMMMM,,.OMMMMMMMM......, 7MM+?+++++++++?+DM$ .............MMMMMMMMMMMMMM ,,......MMM$................................................................................................
.....MMMMM?,.......MMMMM..NMMMMMMMMM.,...,$M7++++++++++++++++++M$ .........MMMMMMMMMMMMMMMMMN .,....MMM$................................................................................................
.....MMMMM?,.......MMMMMMMMMMM8..,,,.,..,MM?++++++++++++++++++++MM,,......MMMMMMMM~,.+MMMMMMMM......MMM$................................................................................................
.....MMMMM?,.......MMMMMMMMZ ,,.......MMMMMMMMMMMMMDZZZZMMMMMMMMMMMMM ...MMMMMM,,,....., MMMMMM.....MMM$................................,.,,............................................................
.....MMMMM?,.......MMMMMMM:............MMMMMMMMMMMMMMMMMMMMMMMMMMMMM....MMMMMD,...........MMMMMM.,..MMM$...:MMMMMMMM:,........8MMM:.,DMMMMM,......?MMMMMMMMI.........MMMM......... MMM,.........MMMI....
.....MMMMM?,.......MMMMMM+............,M?+MMMMMMMMMM++?DMMMMMMMMM?+M,...MMMMM,.............MMMMM,,..MMM$,NMMMMMMMMMMMM8,.,....MMMM,NMMMMMMM,..,,MMMMMMMMMMMMMM.,.....MMMM.........7MMM7.........MMM$....
.....MMMMM?,.......MMMMMM,............,M?++MMMMMMMM7++++MMMMMMMM$??MM,,+MMMMM,.............MMMMM=...MMM$,MMMZ...,?MMMMMM,.....MMMMMMMMM,......DMMMMM:,....MMMMMN,....MMMM.........7MMM7.........MMM$....
.....MMMMM?,.......MMMMMM.............MM+??+MMMMMMM?++++MMMMMMMD??+$M,.MMMMM?.............,MMMMM?...MMM$,M,.,...,,,,MMMMM,....MMMMMM,,,,....,MMMMM,..,....,.MDNN$....MMMM.........7MMM7.........MMM$....
.....MMMMM?,.......MMMMM?,............MM??++???????++++++?????+++++7M..$MMMM,.............,?MMMM.,..MMM$.............OMMMM....MMMMM.........$MMMM,....... MMMMMM.,...MMMM.........7MMM7.........MMM$....
.....MMMMM?,.......MMMMM=,............NM?+++++++++++++++++++++++++?$M..MMMMM+,............,+MMMM+,..NMN$..............MMMM+,..MMMMM.........MMMM......,?MMMMM?.,.....MMMM.........7MMM7.........MMM$....
.....MMMMM?,.......MMMMM,,............,M+?+++++++?++++++++?+?++++++M7,,DMMMM:...............MMMM:,..MMMN.,............$MMM7...MMMM=.........MMMM....,DMMMMM..........MMMM.........7MMM7.........MMM$....
.....MMMMM?,.......MMMMM,,............:M$?++++?MM+++++++++DM?+++++?M,,,DMMMM+,..............MMMM+,..MMMM.,............?NMM?,..ZMMM,,........MMMM.,.MMMMMM,,..........MMMM,........7MMM7.........MMM$....
.....MMMMM?,.......MMMMM,,.............,M=++++++DMD++?++DMM+++++++M:...$MMMM.,..............MMMM ,..MMMM..............OMMM,,..OMMM,,........MMMM.,MMMM?,,......MNZ,,,MMMM.........IMMM?,........MMM?....
.....MMMMM?,.......MMMMM,,...............M+?+++++?+ZMMMN+++?+++++M7,...$MMMM................MMMM.,..=MMMN,..........,,MNMM.,..OMMM,.........?MMMI.,M..........,MMM,.,NMMM,........IMMMI.........MMM?....
.....MMMMM?,.......MMMMM,,................M7+?+++++++++++++++++IM,,....$MMMM,...............MMMM,....MMMMN.,......,,.MMMM,....OMMM,,........,MMMMN..........,+MMM,...,MMMN,.....,,MMMMM,,,.....MMMM.....
.....MMMMM?,.......MMMMM,,................,MM++++?++++++++????MM.......$MMMM,...............MMMM,.....MNMMM$,......MMMMM .....OMMM,,..........MMMMM~......,,MMMM ,....MMMMM,,,..~MMMMMMM~,,,.,MMMMM.....
.....MMMMM?,.......MMMMM,,.................,,MMD+++++++++++$MM,.,......$MMMM,...............MMMM,.....,+NMMMMMMMMMMMMMM..,....OMMM,,.......... +NMMMMMMMMMMMMMM,.......MMMMMMMMMMMMN,NMMMMMMMMMMMN,.....
.....MMMMM?........MMMMM,,.....................::MMMMMMMMM$.,.........,ZMMMM,,..............MMMM,,......, MMMMMMMMMM:.,,......+MMM................MMMMMMMMMM7,,,......,.,MMMMMMMMN.:...MMMMMMMMM,,......
..........,.......,,.....,.........................,,,,.,...................................................,.,,.,,,...........,,,..................,,..,,,,..............,,..,,,.......,,.,,,,.........
........................................................................................................................................................................................................
]]

" + minified, LuaBytecodeEncoding);
				}
				else
				{
					File.WriteAllText(outLua, minified, LuaBytecodeEncoding);
				}

				// Clean up intermediate pipeline artifacts. t1.lua/luac.out in particular
				// contain a far-less-obfuscated version of the script (string-encrypted
				// but not yet virtualized) and shouldn't be left lying around next to out.lua.
				foreach (string temp in new[] { l, t0, t1, t2, t3 })
				{
					if (File.Exists(temp))
						File.Delete(temp);
				}

				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine("ERROR");
				Console.WriteLine(e);

				error = e.ToString();
				return false;
			}
		}
	}
}
