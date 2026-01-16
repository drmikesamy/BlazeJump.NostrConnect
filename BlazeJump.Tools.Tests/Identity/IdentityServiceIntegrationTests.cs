using BlazeJump.Tools.Services.Identity;
using BlazeJump.Tools.Services.Connections;
using BlazeJump.Tools.Services.Message;
using BlazeJump.Tools.Services.Crypto;
using BlazeJump.Tools.Services.Logging;
using BlazeJump.Tools.Services.Persistence;
using BlazeJump.Tools.Enums;
using BlazeJump.Tools.Models;
using BlazeJump.Tools.Models.NostrConnect;
using BlazeJump.Tools.Tests.Mocks;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using NostrConnect.Web.Services.Identity;
using NostrConnect.Maui.Services.Identity;

namespace BlazeJump.Tools.Tests.Identity
{
    public class IdentityServiceIntegrationTests
    {
        private readonly IRelayManager _relayManager;
        private readonly IMessageService _messageService;
        private readonly ICryptoService _cryptoService;
        private readonly ILoggingService _loggingService;
        private readonly WebIdentityService _webService;
        private readonly NativeIdentityService _nativeService;

        public IdentityServiceIntegrationTests()
        {
            _relayManager = new MockRelayManager();
            _messageService = new LoopbackMessageService();
            _cryptoService = new MockCryptoService();
            _loggingService = new MockLoggingService();
            
            _webService = new WebIdentityService(_relayManager, _messageService, _cryptoService, _loggingService);
            _webService.ActiveUserProfile = new UserProfile { PublicKey = "web-test-key" };
            
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
            var session = _webService.ActiveUserProfile!.Sessions.FirstOrDefault();
            Assert.NotNull(session);
            var webPubKey = session.OurPubkey;
            
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
                Assert.True(session.IsConnected, "Web session should be connected");
                Assert.Equal(SessionStatusEnum.Connected, session.Status);
                Assert.Equal(androidPubKey, session.TheirPubkey);

                var androidSession = _nativeService.ActiveUserProfile.Sessions.FirstOrDefault(s => s.TheirPubkey == webPubKey);
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

            var session = _webService.ActiveUserProfile!.Sessions.FirstOrDefault();
            Assert.NotNull(session);
            var webPubKey = session.OurPubkey;
            var androidPubKey = _nativeService.ActiveUserProfile!.PublicKey;

            // 1. Web Pings Android
            bool webPongReceived = false;
            _webService.PingReceived += (s, r) => webPongReceived = true;

            await _webService.SendPing(session);
            await Task.Delay(500);

            Assert.True(webPongReceived, "Web should receive Pong from Android");

            // 2. Android Pings Web
            bool androidPongReceived = false;
            _nativeService.PingReceived += (s, r) => androidPongReceived = true;
            
            var androidSession = _nativeService.ActiveUserProfile.Sessions.FirstOrDefault(s => s.TheirPubkey == webPubKey);
            Assert.NotNull(androidSession);
            await _nativeService.SendPing(androidSession);
            
            // Wait for processing
            await Task.Delay(500);
            
            Assert.True(androidPongReceived, "Android should receive Pong from Web and trigger event");
        }

        [Fact]
        public async Task Test_Disconnect_FromWeb()
        {
            // Establish Connection
            await Test_Handshake_Success();
            var session = _webService.ActiveUserProfile!.Sessions.FirstOrDefault();
            Assert.NotNull(session);
            var webPubKey = session.OurPubkey;

            Assert.True(session.IsConnected);

            // Web sends disconnect
            await _webService.SendDisconnect(session);
            await Task.Delay(500);

            // Assert - session should be removed or marked as disconnected
            var stillExists = _webService.ActiveUserProfile.Sessions.Any(s => s.OurPubkey == webPubKey && s.IsConnected);
            Assert.False(stillExists, "Session should no longer be connected");
            
            // Verify Android side removed session or marked disconnected
            var androidSession = _nativeService.ActiveUserProfile!.Sessions.FirstOrDefault(s => s.TheirPubkey == webPubKey);
            Assert.Null(androidSession); // Disconnect removes it in Native implementation
        }
        
    }
}
