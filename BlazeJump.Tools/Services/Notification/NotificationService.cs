namespace BlazeJump.Tools.Services.Notification
{
	/// <summary>
	/// Implements notification services for UI state updates.
	/// </summary>
	public class NotificationService : INotificationService
	{
		/// <summary>
		/// Occurs when the state needs to be updated.
		/// </summary>
		public event EventHandler? UpdateState;

		private bool _loading { get; set; } = false;

		/// <summary>
		/// Gets or sets a value indicating whether the application is loading.
		/// </summary>
		public bool Loading
		{
			get
			{
				return _loading;
			}
			set
			{
				if (_loading != value)
				{
					_loading = value;
					UpdateState?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		/// <summary>
		/// Triggers a state update notification.
		/// </summary>
		public void UpdateTheState()
		{
#if ANDROID
			MainThread.BeginInvokeOnMainThread(() =>
			{
				UpdateState?.Invoke(this, EventArgs.Empty);
			});
#else
			UpdateState?.Invoke(this, EventArgs.Empty);
#endif

		}
	}
}