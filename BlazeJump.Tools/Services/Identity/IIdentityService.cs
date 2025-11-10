using BlazeJump.Tools.Enums;
using BlazeJump.Tools.Models;

namespace BlazeJump.Tools.Services.Identity
{
	/// <summary>
	/// Provides identity services for Nostr Connect authentication.
	/// </summary>
	public interface IIdentityService
	{
		/// <summary>
		/// Occurs when a QR connect request is received.
		/// </summary>
		event EventHandler<QrConnectEventArgs> QrConnectReceived;

		/// <summary>
		/// Raises the QR connect received event.
		/// </summary>
		/// <param name="e">The event arguments containing connection details.</param>
		void OnQrConnectReceived(QrConnectEventArgs e);

		/// <summary>
		/// Fetches the login scan response from relays.
		/// </summary>
		/// <param name="payload">The QR connect payload.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task FetchLoginScanResponse(QrConnectEventArgs payload);

		/// <summary>
		/// Gets or sets the current platform.
		/// </summary>
		PlatformEnum Platform { get; set; }
	}
}
