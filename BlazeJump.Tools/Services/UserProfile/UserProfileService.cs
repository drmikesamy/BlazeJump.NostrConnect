using BlazeJump.Tools.Models;
using BlazeJump.Tools.Models.Crypto;
using BlazeJump.Tools.Services.Crypto;

namespace BlazeJump.Tools.Services.UserProfile
{
	/// <summary>
	/// Implements user profile management services.
	/// </summary>
	public class UserProfileService : IUserProfileService
	{
		private ICryptoService _cryptoService;

		/// <summary>
		/// Initializes a new instance of the <see cref="UserProfileService"/> class.
		/// </summary>
		/// <param name="cryptoService">The crypto service for key management.</param>
		public UserProfileService(ICryptoService cryptoService) {
			_cryptoService = cryptoService;
		}

		/// <summary>
		/// Gets or sets the current user.
		/// </summary>
		public User User { get; set; } = new User();

		/// <summary>
		/// Gets or sets a value indicating whether the user is logged in.
		/// </summary>
		public bool IsLoggedIn { get; set; }

		/// <summary>
		/// Gets or sets the dictionary of users.
		/// </summary>
		public Dictionary<string, User> UserList { get; set; } = new Dictionary<string, User>();

		/// <summary>
		/// Gets or sets the current user's Nostr public key.
		/// </summary>
		public string NPubKey { get; set; } = string.Empty;

		/// <summary>
		/// Initializes the user profile service.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		public Task Init()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Changes the user's profile picture.
		/// </summary>
		/// <param name="imageUrl">The new profile picture URL.</param>
		public void ChangeProfilePicture(string imageUrl)
        {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Logs in the user.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		public Task Login()
        {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Logs out the user.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		public Task Logout()
        {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Registers a new user.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		public Task Register()
        {
            throw new NotImplementedException();
        }
    }
}
