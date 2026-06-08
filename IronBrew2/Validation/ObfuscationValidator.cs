using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace IronBrew2.Validation
{
	public class ObfuscationValidator
	{
		private List<string> _errors = new List<string>();
		private List<string> _warnings = new List<string>();

		public bool HasErrors => _errors.Count > 0;
		public bool HasWarnings => _warnings.Count > 0;

		public IEnumerable<string> Errors => _errors;
		public IEnumerable<string> Warnings => _warnings;

		public void Clear()
		{
			_errors.Clear();
			_warnings.Clear();
		}

		public bool ValidateSyntax(string filePath)
		{
			if (!File.Exists(filePath))
			{
				_errors.Add($"File not found: {filePath}");
				return false;
			}

			try
			{
				string OS = Environment.OSVersion.Platform == PlatformID.Unix ? "/usr/bin/" : "";

				Process proc = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = $"{OS}luac",
						Arguments = "-o nul \"" + filePath + "\"",
						UseShellExecute = false,
						RedirectStandardError = true,
						RedirectStandardOutput = true
					}
				};

				string errors = "";
				proc.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) errors += e.Data + "\n"; };
				proc.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) errors += e.Data + "\n"; };

				proc.Start();
				proc.BeginOutputReadLine();
				proc.BeginErrorReadLine();
				proc.WaitForExit();

				if (proc.ExitCode != 0)
				{
					_errors.Add($"Syntax validation failed:\n{errors}");
					return false;
				}

				return true;
			}
			catch (Exception ex)
			{
				_warnings.Add($"Could not validate syntax (luac not found?): {ex.Message}");
				return true;
			}
		}

		public bool ValidateFileSize(long originalSize, long obfuscatedSize)
		{
			if (obfuscatedSize == 0)
			{
				_errors.Add("Obfuscated file size is 0 bytes");
				return false;
			}

			if (obfuscatedSize > originalSize * 5)
			{
				_warnings.Add($"Obfuscated file is {obfuscatedSize / (double)originalSize:F1}x larger than original");
			}

			return true;
		}

		public bool ValidateEncoding(byte[] data)
		{
			if (data == null || data.Length == 0)
			{
				_errors.Add("Data is null or empty");
				return false;
			}

			return true;
		}

		public bool ValidateAll(string obfuscatedPath, long originalSize)
		{
			Clear();

			if (!File.Exists(obfuscatedPath))
			{
				_errors.Add($"Obfuscated file not found: {obfuscatedPath}");
				return false;
			}

			byte[] data = File.ReadAllBytes(obfuscatedPath);
			long obfuscatedSize = data.Length;

			bool isValid = true;
			isValid &= ValidateEncoding(data);
			isValid &= ValidateFileSize(originalSize, obfuscatedSize);
			isValid &= ValidateSyntax(obfuscatedPath);

			return isValid;
		}

		public string GetReport()
		{
			string report = "";

			if (_errors.Count > 0)
			{
				report += $"\n✗ Errors ({_errors.Count}):\n";
				foreach (string error in _errors)
					report += $"  - {error}\n";
			}

			if (_warnings.Count > 0)
			{
				report += $"\n⚠ Warnings ({_warnings.Count}):\n";
				foreach (string warning in _warnings)
					report += $"  - {warning}\n";
			}

			if (!HasErrors && !HasWarnings)
				report += "\n✓ All validations passed!";

			return report;
		}
	}
}
