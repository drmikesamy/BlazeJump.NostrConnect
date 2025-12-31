namespace BlazeJump.Tools.Services.Logging
{
	public interface ILoggingService
	{
		event EventHandler<string>? LogAdded;
		void Log(string message);
		void Clear();
		List<string> GetLogs();
	}
}
