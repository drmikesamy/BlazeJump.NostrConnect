using System.Collections.Concurrent;

namespace BlazeJump.Tools.Services.Logging
{
	public class LoggingService : ILoggingService
	{
		private readonly ConcurrentQueue<string> _logs = new();
		private const int MaxLogs = 10000;

		public event EventHandler<string>? LogAdded;

		public void Log(string message)
		{
			var timestampedMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
			_logs.Enqueue(timestampedMessage);

			// Keep only last MaxLogs entries
			while (_logs.Count > MaxLogs)
			{
				_logs.TryDequeue(out _);
			}

			LogAdded?.Invoke(this, timestampedMessage);
		}

		public void Clear()
		{
			_logs.Clear();
		}

		public List<string> GetLogs()
		{
			return _logs.ToList();
		}
	}
}
