using BlazeJump.Tools.Services.Logging;

namespace BlazeJump.Tools.Tests.Mocks
{
    public class MockLoggingService : ILoggingService
    {
        private readonly List<string> _logs = new();

        public event EventHandler<string>? LogAdded;

        public void Log(string message)
        {
            _logs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {message}");
            LogAdded?.Invoke(this, message);
        }

        public void Clear()
        {
            _logs.Clear();
        }

        public List<string> GetLogs()
        {
            return new List<string>(_logs);
        }
    }
}
