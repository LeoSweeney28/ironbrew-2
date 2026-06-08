using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace IronBrew2.Utilities
{
	public class PipelineMetrics
	{
		private Dictionary<string, TimeSpan> _stageTimes = new Dictionary<string, TimeSpan>();
		private Stopwatch _totalStopwatch = new Stopwatch();

		public void StartTotal()
		{
			_totalStopwatch.Restart();
		}

		public void StopTotal()
		{
			_totalStopwatch.Stop();
		}

		public void RecordStage(string stageName, TimeSpan duration)
		{
			if (stageName == null)
				throw new ArgumentNullException(nameof(stageName));

			_stageTimes[stageName] = duration;
		}

		public void RecordStage(string stageName, long milliseconds)
		{
			RecordStage(stageName, TimeSpan.FromMilliseconds(milliseconds));
		}

		public TimeSpan GetStageTime(string stageName)
		{
			return _stageTimes.TryGetValue(stageName, out var time) ? time : TimeSpan.Zero;
		}

		public TimeSpan TotalTime => _totalStopwatch.Elapsed;

		public string GetReport()
		{
			if (_stageTimes.Count == 0)
				return "No metrics recorded";

			var sortedStages = _stageTimes.OrderByDescending(x => x.Value);
			string report = "\n=== Pipeline Performance Metrics ===\n";

			foreach (var stage in sortedStages)
			{
				double percent = TotalTime.TotalMilliseconds > 0
					? (stage.Value.TotalMilliseconds / TotalTime.TotalMilliseconds) * 100
					: 0;

				report += $"{stage.Key,-25} {stage.Value.TotalSeconds,8:F2}s ({percent,5:F1}%)\n";
			}

			report += $"{new string('-', 45)}\n";
			report += $"{"Total",25} {TotalTime.TotalSeconds,8:F2}s (100.0%)\n";
			report += "====================================";

			return report;
		}
	}

	public class ScopedTimer : IDisposable
	{
		private readonly string _name;
		private readonly Action<string, TimeSpan> _callback;
		private readonly Stopwatch _stopwatch;

		public ScopedTimer(string name, Action<string, TimeSpan> callback)
		{
			_name = name;
			_callback = callback;
			_stopwatch = Stopwatch.StartNew();
		}

		public void Dispose()
		{
			_stopwatch.Stop();
			_callback?.Invoke(_name, _stopwatch.Elapsed);
		}
	}
}
