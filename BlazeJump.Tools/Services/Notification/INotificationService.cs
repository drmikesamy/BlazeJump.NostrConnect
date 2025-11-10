namespace BlazeJump.Tools.Services.Notification
{
	/// <summary>
	/// Provides notification services for UI state updates.
	/// </summary>
	public interface INotificationService
	{
		/// <summary>
		/// Occurs when the state needs to be updated.
		/// </summary>
		event EventHandler UpdateState;

		/// <summary>
		/// Gets or sets a value indicating whether the application is loading.
		/// </summary>
		bool Loading { get; set; }

		/// <summary>
		/// Triggers a state update notification.
		/// </summary>
		void UpdateTheState();
	}
}