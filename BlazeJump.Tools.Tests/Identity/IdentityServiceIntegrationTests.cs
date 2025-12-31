using BlazeJump.Tools.Enums;
using System.IO;
using BlazeJump.Tools.Services.Logging;
using BlazeJump.Tools.Tests.Mocks;
using NostrConnect.Maui.Services.Identity;
using NostrConnect.Web.Services.Identity;

using Xunit;

namespace BlazeJump.Tools.Tests.Identity
{
    public class IdentityServiceIntegrationTests
    {
        private readonly LoopbackMessageService _messageService;
        private readonly MockCryptoService _cryptoService;
        private readonly MockRelayManager _relayManager;
        private readonly LoggingService _loggingService;

        private readonly WebIdentityService _webService;
        private readonly NativeIdentityService _nativeService;

        public IdentityServiceIntegrationTests()
        {
            _messageService = new LoopbackMessageService();
            _cryptoService = new MockCryptoService();
            _relayManager = new MockRelayManager();
            _loggingService = new LoggingService();

            _webService = new WebIdentityService(_relayManager, _messageService, _cryptoService, _loggingService);

            var mockDataService = new MockNostrDataService();
            _nativeService = new NativeIdentityService(_relayManager, _messageService, _cryptoService, _loggingService, mockDataService);
        }

        [Fact]
        public async Task Test_Handshake_Success()
        {
            // 1. Setup Android User Profile
            await _nativeService.CreateUserProfile();
            var androidPubKey = _nativeService.ActiveUserProfile!.PublicKey;

            // 2. Web creates session
            var connectionUrl = await _webService.CreateNewSession();
            var session = _webService.Session;
            var webPubKey = session.WebPubkey;
            
            // Parse URL to get secret
            var secret = connectionUrl.Split("secret=")[1].Split("&")[0];
            secret = Uri.UnescapeDataString(secret);

            // 3. Android scans QR (simulated)
            // This triggers: Android -> Connect Request -> Web -> Ack Response -> Android
            await _nativeService.OnQrConnectReceived(webPubKey, new List<string> { "wss://mock.com" }, secret, new List<string>());

            // Wait for async message processing
            await Task.Delay(500);

            // 4. Assertions
            try 
            {
                Assert.True(_webService.Session.IsConnected, "Web session should be connected");
                Assert.Equal(SessionStatusEnum.Connected, _webService.Session.Status);
                Assert.Equal(androidPubKey, _webService.Session.UserPublicKey);

                var androidSession = _nativeService.ActiveUserProfile.Sessions.FirstOrDefault(s => s.WebPubkey == webPubKey);
                Assert.NotNull(androidSession);
                Assert.True(androidSession.IsConnected, "Android session should be connected");
            }
            catch
            {
                Console.WriteLine("DEBUG LOGS:");
                foreach(var log in _loggingService.GetLogs()) Console.WriteLine(log);
                throw;
            }
        }

        [Fact]
        public async Task Test_Ping_Bidirectional()
        {
            // Establish Connection first
            await Test_Handshake_Success();

            var session = _webService.Session;
            var webPubKey = session.WebPubkey;
            var androidPubKey = _nativeService.ActiveUserProfile!.PublicKey;

            // 1. Web Pings Android
            bool webPongReceived = false;
            _webService.PingReceived += (s, r) => webPongReceived = true;

            await _webService.SendPing();
            await Task.Delay(500);

            Assert.True(webPongReceived, "Web should receive Pong from Android");

            // 2. Android Pings Web
            bool androidPongReceived = false;
            _nativeService.PongReceived += (s, r) => androidPongReceived = true;
            
            await _nativeService.SendPingToClient(webPubKey);
            // Wait for processing
            await Task.Delay(500);
            
            Assert.True(androidPongReceived, "Android should receive Pong from Web and trigger event");
        }

        [Fact]
        public async Task Test_Disconnect_FromWeb()
        {
            // Establish Connection
            await Test_Handshake_Success();
            var webPubKey = _webService.Session.WebPubkey;

            Assert.True(_webService.Session.IsConnected);

            // Web sends disconnect
            await _webService.SendDisconnect();
            await Task.Delay(500);

            // Assert
            Assert.Equal(SessionStatusEnum.Disconnected, _webService.Session.Status);
            
            // Verify Android side removed session or marked disconnected
            var androidSession = _nativeService.ActiveUserProfile!.Sessions.FirstOrDefault(s => s.WebPubkey == webPubKey);
            Assert.Null(androidSession); // Disconnect removes it in Native implementation
        }
        
    }
}
