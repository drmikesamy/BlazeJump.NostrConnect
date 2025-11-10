using BlazeJump.Tools.Models;
using BlazeJump.Tools.Models.Crypto;

namespace BlazeJump.Tools.Services.UserProfile
{
	/// <summary>
	/// Provides user profile management services.
	/// </summary>
	public interface IUserProfileService
	{
		/// <summary>
		/// Gets or sets the current user.
		/// </summary>
		User User { get; set; }

		/// <summary>
		/// Gets or sets the dictionary of users.
		/// </summary>
		Dictionary<string, User> UserList { get; set; }

		/// <summary>
		/// Gets or sets the current user's Nostr public key.
		/// </summary>
		string NPubKey { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the user is logged in.
		/// </summary>
		bool IsLoggedIn { get; set; }

		/// <summary>
		/// Initializes the user profile service.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task Init();

		/// <summary>
		/// Changes the user's profile picture.
		/// </summary>
		/// <param name="imageUrl">The new profile picture URL.</param>
		void ChangeProfilePicture(string imageUrl);

		/// <summary>
		/// Logs in the user.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task Login();

		/// <summary>
		/// Logs out the user.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task Logout();

		/// <summary>
		/// Registers a new user.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task Register();
	}
}