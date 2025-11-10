using AutoMapper;
using BlazeJump.Tools.Services.Connections;
using BlazeJump.Tools.Services.Connections.Providers;
using BlazeJump.Tools.Services.Identity;
using BlazeJump.Tools.Services.Message;
using BlazeJump.Tools.Services.Notification;
using BlazeJump.Tools.Services.UserProfile;

namespace BlazeJump.Tools
{
	/// <summary>
	/// Provides extension methods for configuring BlazeJump common services.
	/// </summary>
	public static class CommonServices
	{
		/// <summary>
		/// Configures and registers all BlazeJump common services with the dependency injection container.
		/// </summary>
		/// <param name="services">The service collection to add services to.</param>
		/// <remarks>
		/// This method registers the following services as singletons:
		/// <list type="bullet">
		/// <item><description>IIdentityService - Manages Nostr identities and keys</description></item>
		/// <item><description>IMessageService - Handles Nostr event messaging</description></item>
		/// <item><description>INotificationService - Manages application notifications</description></item>
		/// <item><description>IRelayManager - Manages connections to Nostr relays</description></item>
		/// <item><description>IUserProfileService - Handles user profile operations</description></item>
		/// <item><description>IMapper - AutoMapper instance for object mapping</description></item>
		/// </list>
		/// </remarks>
		/// <example>
		/// <code>
		/// builder.Services.ConfigureServices();
		/// </code>
		/// </example>
		public static void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<IIdentityService, IdentityService>();
			services.AddSingleton<IMessageService, MessageService>();
			services.AddSingleton<INotificationService, NotificationService>();
			services.AddSingleton<IRelayManager, RelayManager>(
				_ => new RelayManager(new RelayConnectionProvider()));
			services.AddSingleton<IUserProfileService, UserProfileService>();

			var mapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddMaps("BlazeJump.Tools");
			});
			mapperConfig.AssertConfigurationIsValid();

			IMapper mapper = mapperConfig.CreateMapper();
			services.AddSingleton(mapper);
		}
	}
}