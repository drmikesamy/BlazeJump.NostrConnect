using System.Text;

namespace BlazeJump.Tools.Builders
{
	/// <summary>
	/// Builder class for constructing Nostr Connect connection URIs using a fluent API.
	/// Implements NIP-46 (Nostr Connect) protocol for remote signer connections.
	/// </summary>
	public class NostrConnectUriBuilder
	{
		private string _clientPubKey = string.Empty;
		private readonly List<string> _relays = new List<string>();
		private string _secret = string.Empty;
		private readonly List<string> _perms = new List<string>();
		private string? _name;
		private string? _url;
		private string? _image;

		/// <summary>
		/// Sets the client public key (origin) for the connection.
		/// </summary>
		/// <param name="clientPubKey">The client's public key in hex format.</param>
		/// <returns>The current NostrConnectUriBuilder instance for method chaining.</returns>
		/// <exception cref="ArgumentException">Thrown when clientPubKey is null or empty.</exception>
		public NostrConnectUriBuilder WithClientPubKey(string clientPubKey)
		{
			if (string.IsNullOrWhiteSpace(clientPubKey))
				throw new ArgumentException("Client public key cannot be null or empty.", nameof(clientPubKey));

			_clientPubKey = clientPubKey;
			return this;
		}

		/// <summary>
		/// Adds a relay URL where the client is listening for responses from the remote-signer.
		/// </summary>
		/// <param name="relayUrl">The WebSocket URL of the relay (e.g., "wss://relay.example.com").</param>
		/// <returns>The current NostrConnectUriBuilder instance for method chaining.</returns>
		/// <exception cref="ArgumentException">Thrown when relayUrl is null or empty.</exception>
		public NostrConnectUriBuilder AddRelay(string relayUrl)
		{
			if (string.IsNullOrWhiteSpace(relayUrl))
				throw new ArgumentException("Relay URL cannot be null or empty.", nameof(relayUrl));

			_relays.Add(relayUrl);
			return this;
		}

		/// <summary>
		/// Adds multiple relay URLs where the client is listening for responses.
		/// </summary>
		/// <param name="relayUrls">The collection of relay URLs.</param>
		/// <returns>The current NostrConnectUriBuilder instance for method chaining.</returns>
		public NostrConnectUriBuilder AddRelays(IEnumerable<string> relayUrls)
		{
			if (relayUrls == null)
				throw new ArgumentNullException(nameof(relayUrls));

			foreach (var relay in relayUrls)
			{
				if (!string.IsNullOrWhiteSpace(relay))
					_relays.Add(relay);
			}
			return this;
		}

		/// <summary>
		/// Sets the secret value that the remote-signer should return to avoid connection spoofing.
		/// </summary>
		/// <param name="secret">A short random string that will be validated by the client.</param>
		/// <returns>The current NostrConnectUriBuilder instance for method chaining.</returns>
		/// <exception cref="ArgumentException">Thrown when secret is null or empty.</exception>
		public NostrConnectUriBuilder WithSecret(string secret)
		{
			if (string.IsNullOrWhiteSpace(secret))
				throw new ArgumentException("Secret cannot be null or empty.", nameof(secret));

			_secret = secret;
			return this;
		}

		/// <summary>
		/// Generates a random 8-character secret and sets it.
		/// </summary>
		/// <returns>The current NostrConnectUriBuilder instance for method chaining.</returns>
		public NostrConnectUriBuilder WithRandomSecret()
		{
			_secret = Guid.NewGuid().ToString().Substring(0, 8);
			return this;
		}

		/// <summary>
		/// Adds a permission that the client is requesting be approved by the remote-signer.
		/// </summary>
		/// <param name="permission">The permission string (e.g., "sign_event:1", "nip44_encrypt").</param>
		/// <returns>The current NostrConnectUriBuilder instance for method chaining.</returns>
		public NostrConnectUriBuilder AddPermission(string permission)
		{
			if (!string.IsNullOrWhiteSpace(permission))
				_perms.Add(permission);

			return this;
		}

		/// <summary>
		/// Adds multiple permissions that the client is requesting.
		/// </summary>
		/// <param name="permissions">The collection of permission strings.</param>
		/// <returns>The current NostrConnectUriBuilder instance for method chaining.</returns>
		public NostrConnectUriBuilder AddPermissions(IEnumerable<string> permissions)
		{
			if (permissions == null)
				throw new ArgumentNullException(nameof(permissions));

			foreach (var perm in permissions)
			{
				if (!string.IsNullOrWhiteSpace(perm))
					_perms.Add(perm);
			}
			return this;
		}

		/// <summary>
		/// Sets the name of the client application.
		/// </summary>
		/// <param name="name">The application name.</param>
		/// <returns>The current NostrConnectUriBuilder instance for method chaining.</returns>
		public NostrConnectUriBuilder WithName(string name)
		{
			_name = name;
			return this;
		}

		/// <summary>
		/// Sets the canonical URL of the client application.
		/// </summary>
		/// <param name="url">The application URL.</param>
		/// <returns>The current NostrConnectUriBuilder instance for method chaining.</returns>
		public NostrConnectUriBuilder WithUrl(string url)
		{
			_url = url;
			return this;
		}

		/// <summary>
		/// Sets the image URL representing the client application.
		/// </summary>
		/// <param name="imageUrl">A small image URL for the application.</param>
		/// <returns>The current NostrConnectUriBuilder instance for method chaining.</returns>
		public NostrConnectUriBuilder WithImage(string imageUrl)
		{
			_image = imageUrl;
			return this;
		}

		/// <summary>
		/// Gets the secret value that was set or generated.
		/// </summary>
		/// <returns>The secret string, or empty if not set.</returns>
		public string GetSecret()
		{
			return _secret;
		}

		/// <summary>
		/// Gets the relays that were set or generated.
		/// </summary>
		/// <returns>The relays.</returns>
		public List<string> GetRelays()
		{
			return _relays;
		}

		/// <summary>
		/// Gets the client public key.
		/// </summary>
		/// <returns>The client public key string, or empty if not set.</returns>
		public string GetClientPubKey()
		{
			return _clientPubKey;
		}

		/// <summary>
		/// Gets the permissions that were set.
		/// </summary>
		/// <returns>The list of permissions.</returns>
		public List<string> GetPermissions()
		{
			return _perms;
		}

		/// <summary>
		/// Gets the application name.
		/// </summary>
		/// <returns>The application name, or null if not set.</returns>
		public string? GetName()
		{
			return _name;
		}

		/// <summary>
		/// Gets the application URL.
		/// </summary>
		/// <returns>The application URL, or null if not set.</returns>
		public string? GetUrl()
		{
			return _url;
		}

		/// <summary>
		/// Gets the image URL.
		/// </summary>
		/// <returns>The image URL, or null if not set.</returns>
		public string? GetImage()
		{
			return _image;
		}

		/// <summary>
		/// Builds and returns the Nostr Connect URI string.
		/// </summary>
		/// <returns>A fully constructed nostrconnect:// URI with all parameters.</returns>
		/// <exception cref="InvalidOperationException">Thrown when required fields are missing.</exception>
		public string Build()
		{
			// Validate required fields
			if (string.IsNullOrWhiteSpace(_clientPubKey))
				throw new InvalidOperationException("Client public key is required. Call WithClientPubKey() first.");

			if (_relays.Count == 0)
				throw new InvalidOperationException("At least one relay URL is required. Call AddRelay() first.");

			if (string.IsNullOrWhiteSpace(_secret))
				throw new InvalidOperationException("Secret is required. Call WithSecret() or WithRandomSecret() first.");

			// Build the URI
			var uriBuilder = new StringBuilder();
			uriBuilder.Append("nostrconnect://");
			uriBuilder.Append(_clientPubKey);

			// Add query parameters
			var queryParams = new List<string>();

			// Add relays (multiple relay parameters)
			foreach (var relay in _relays)
			{
				queryParams.Add($"relay={Uri.EscapeDataString(relay)}");
			}

			// Add secret
			queryParams.Add($"secret={Uri.EscapeDataString(_secret)}");

			// Add perms if specified
			if (_perms.Count > 0)
			{
				var permsString = string.Join(",", _perms);
				queryParams.Add($"perms={Uri.EscapeDataString(permsString)}");
			}

			// Add name if specified
			if (!string.IsNullOrWhiteSpace(_name))
			{
				queryParams.Add($"name={Uri.EscapeDataString(_name)}");
			}

			// Add url if specified
			if (!string.IsNullOrWhiteSpace(_url))
			{
				queryParams.Add($"url={Uri.EscapeDataString(_url)}");
			}

			// Add image if specified
			if (!string.IsNullOrWhiteSpace(_image))
			{
				queryParams.Add($"image={Uri.EscapeDataString(_image)}");
			}

			// Append query string
			if (queryParams.Count > 0)
			{
				uriBuilder.Append("?");
				uriBuilder.Append(string.Join("&", queryParams));
			}

			return uriBuilder.ToString();
		}

		/// <summary>
		/// Parses a Nostr Connect URI string and populates a new builder instance.
		/// </summary>
		/// <param name="uri">The nostrconnect:// URI to parse.</param>
		/// <returns>A NostrConnectUriBuilder populated with the parsed data.</returns>
		/// <exception cref="ArgumentException">Thrown when URI is invalid or missing required fields.</exception>
		public static NostrConnectUriBuilder Parse(string uri)
		{
			if (string.IsNullOrWhiteSpace(uri))
				throw new ArgumentException("URI cannot be null or empty.", nameof(uri));

			if (!uri.StartsWith("nostrconnect://", StringComparison.OrdinalIgnoreCase))
				throw new ArgumentException("URI must start with 'nostrconnect://'.", nameof(uri));

			var builder = new NostrConnectUriBuilder();

			// Remove the protocol
			var uriWithoutProtocol = uri.Substring(15); // "nostrconnect://".Length

			// Split on ? to separate pubkey from query string
			var parts = uriWithoutProtocol.Split(new[] { '?' }, 2);

			if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0]))
				throw new ArgumentException("URI must contain a client public key.", nameof(uri));

			builder.WithClientPubKey(parts[0]);

			// Parse query string if present
			if (parts.Length == 2)
			{
				var queryString = parts[1];
				var queryParams = queryString.Split('&');

				foreach (var param in queryParams)
				{
					var keyValue = param.Split(new[] { '=' }, 2);
					if (keyValue.Length != 2) continue;

					var key = Uri.UnescapeDataString(keyValue[0]);
					var value = Uri.UnescapeDataString(keyValue[1]);

					switch (key.ToLowerInvariant())
					{
						case "relay":
							builder.AddRelay(value);
							break;
						case "secret":
							builder.WithSecret(value);
							break;
						case "perms":
							var permissions = value.Split(',');
							builder.AddPermissions(permissions);
							break;
						case "name":
							builder.WithName(value);
							break;
						case "url":
							builder.WithUrl(value);
							break;
						case "image":
							builder.WithImage(value);
							break;
					}
				}
			}

			return builder;
		}
	}
}
