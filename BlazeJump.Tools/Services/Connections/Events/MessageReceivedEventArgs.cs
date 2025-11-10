using BlazeJump.Tools.Models;

namespace BlazeJump.Tools.Services.Connections.Events
{
	/// <summary>
	/// Event arguments for when a message is received from a relay.
	/// </summary>
	public class MessageReceivedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MessageReceivedEventArgs"/> class.
		/// </summary>
		/// <param name="url">The relay URL that sent the message.</param>
		/// <param name="message">The received message.</param>
		public MessageReceivedEventArgs(string url, NMessage message)
		{
			Url = url;
			Message = message;
		}
		
		/// <summary>
		/// Gets or sets the relay URL that sent the message.
		/// </summary>
		public string Url { get; set; }
		
		/// <summary>
		/// Gets or sets the received message.
		/// </summary>
		public NMessage Message { get; set; }
	}
}