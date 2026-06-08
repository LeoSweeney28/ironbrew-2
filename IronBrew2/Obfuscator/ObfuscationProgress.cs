using System;

namespace IronBrew2.Obfuscator
{
	public enum ObfuscationStage
	{
		SyntaxCheck,
		CommentStripping,
		ConstantEncryption,
		Compilation,
		ControlFlowObfuscation,
		VMGeneration,
		Minification,
		Validation,
		Watermarking,
		Complete
	}

	public class ObfuscationProgress
	{
		public ObfuscationStage CurrentStage { get; set; }
		public string Message { get; set; }
		public int? Progress { get; set; }
		public int? MaxProgress { get; set; }

		public ObfuscationProgress(ObfuscationStage stage, string message, int? progress = null, int? maxProgress = null)
		{
			CurrentStage = stage;
			Message = message;
			Progress = progress;
			MaxProgress = maxProgress;
		}

		public override string ToString()
		{
			string progressStr = (Progress.HasValue && MaxProgress.HasValue)
				? $" ({Progress.Value}/{MaxProgress.Value})"
				: "";
			return $"[{CurrentStage}] {Message}{progressStr}";
		}
	}

	public class ObfuscationStatistics
	{
		public long OriginalFileSize { get; set; }
		public long ObfuscatedFileSize { get; set; }
		public double CompressionPercent
		{
			get
			{
				return OriginalFileSize > 0
					? ((OriginalFileSize - ObfuscatedFileSize) / (double)OriginalFileSize) * 100
					: 0;
			}
		}
		public TimeSpan TotalTime { get; set; }
		public bool IsValid { get; set; }
		public string ValidationErrors { get; set; }

		public ObfuscationStatistics()
		{
			IsValid = true;
			ValidationErrors = "";
		}

		public override string ToString()
		{
			string arrow = CompressionPercent > 0 ? "↓" : "↑";
			string status = IsValid ? "✓ Valid" : "✗ Invalid";
			string errors = IsValid ? "" : $"\nErrors: {ValidationErrors}";

			return $@"
=== Obfuscation Statistics ===
Original Size:     {FormatBytes(OriginalFileSize)}
Obfuscated Size:   {FormatBytes(ObfuscatedFileSize)}
Compression:       {CompressionPercent:F2}% {arrow}
Time Elapsed:      {TotalTime.TotalSeconds:F2}s
Status:            {status}{errors}
==============================";
		}

		private static string FormatBytes(long bytes)
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
	}
}
