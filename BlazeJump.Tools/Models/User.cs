using BlazeJump.Tools.Enums;
using BlazeJump.Tools.Models.Crypto;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace BlazeJump.Tools.Models
{
	/// <summary>
	/// Represents a Nostr user with profile information and associated events.
	/// </summary>
	public class User
	{
		/// <summary>
		/// Gets or sets the user's unique identifier (public key).
		/// </summary>
		[Key]
		[JsonIgnore]
		public string Id { get; set; } = string.Empty;
		
		/// <summary>
		/// Gets or sets the user's display name.
		/// </summary>
		[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
		public string? Username { get; set; }
		
		/// <summary>
		/// Gets or sets the user's biography/about text.
		/// </summary>
		[JsonProperty("about", NullValueHandling = NullValueHandling.Ignore)]
		public string? Bio { get; set; }
		
		/// <summary>
		/// Gets or sets the user's email address (not published to Nostr).
		/// </summary>
		[JsonIgnore]
		public string? Email { get; set; }
		
		/// <summary>
		/// Gets or sets the user's password (not published to Nostr).
		/// </summary>
		[JsonIgnore]
		public string? Password { get; set; }
		
		/// <summary>
		/// Gets or sets the password confirmation field (not published to Nostr).
		/// </summary>
		[JsonIgnore]
		public string? RepeatPassword { get; set; }
		
		/// <summary>
		/// Gets or sets the user's profile picture URL.
		/// </summary>
		[JsonProperty("picture", NullValueHandling = NullValueHandling.Ignore)]
		public string? ProfilePic { get; set; }
		
		/// <summary>
		/// Gets or sets the user's banner/header image URL.
		/// </summary>
		[JsonProperty("banner", NullValueHandling = NullValueHandling.Ignore)]
		public string? Banner { get; set; }
		
		/// <summary>
		/// Gets or sets the collection of events authored by this user.
		/// </summary>
		[JsonIgnore]
		public ICollection<NEvent> Events { get; set; } = new List<NEvent>();
	}

}