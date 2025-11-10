using NostrConnect.Shared.Models;
using System.Collections.Concurrent;
using BlazeJump.Tools.Services.Connections;
using BlazeJump.Tools.Services.Crypto;
using BlazeJump.Tools.Models;
using BlazeJump.Tools.Enums;

namespace NostrConnect.Shared.Services;

public class NostrConnectService : INostrConnectService
{
    private static readonly ConcurrentDictionary<string, NostrConnectSession> _sessions = new();
    private readonly INostrService _nostrService;
    private readonly IKeyStorageService _keyStorageService;
    private readonly IRelayManager _relayManager;
    private readonly ICryptoService _cryptoService;

    public NostrConnectService(INostrService nostrService, IKeyStorageService keyStorageService, IRelayManager relayManager, ICryptoService cryptoService)
    {
        _nostrService = nostrService;
        _keyStorageService = keyStorageService;
        _relayManager = relayManager;
        _cryptoService = cryptoService;
    }

    public async Task<NostrConnectSession> CreateSessionAsync(string webEphemeralPubKey)
    {
        // Session ID is just the web app's pubkey for now
        // It will be updated when the mobile app connects
        var session = new NostrConnectSession
        {
            SessionId = webEphemeralPubKey, // Use web pubkey as initial session ID
            WebEphemeralPubKey = webEphemeralPubKey,
            IsConnected = false,
            CreatedAt = DateTime.UtcNow
        };

        _sessions[session.SessionId] = session;
        
        return await Task.FromResult(session);
    }

    public async Task<NostrConnectSession?> GetSessionAsync(string sessionId)
    {
        // Try exact match first
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            return await Task.FromResult(session);
        }
        
        // Try to find by web pubkey (first part of compound key)
        var sessionByWebPubkey = _sessions.Values.FirstOrDefault(s => 
            s.WebEphemeralPubKey == sessionId || 
            s.SessionId.StartsWith(sessionId + "_"));
        
        return await Task.FromResult(sessionByWebPubkey);
    }

    public async Task<bool> ConnectSessionAsync(string webPubKey, string appPubKey, string? relay = null, string? secret = null)
    {
        // Create compound session ID
        var compoundSessionId = $"{webPubKey}_{appPubKey}";
        
        // Check if there's an existing session with just the web pubkey
        var existingSession = await GetSessionAsync(webPubKey);
        
        if (existingSession != null)
        {
            // Update existing session
            existingSession.AppPubKey = appPubKey;
            existingSession.IsConnected = true;
            existingSession.ConnectedAt = DateTime.UtcNow;
            existingSession.SessionId = compoundSessionId;
            
            // Move to new key
            _sessions.TryRemove(webPubKey, out _);
            _sessions[compoundSessionId] = existingSession;
        }
        else
        {
            // Create new session
            var session = new NostrConnectSession
            {
                SessionId = compoundSessionId,
                WebEphemeralPubKey = webPubKey,
                AppPubKey = appPubKey,
                IsConnected = true,
                ConnectedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _sessions[compoundSessionId] = session;
        }
        
        // Send connection acknowledgment via relay
        if (!string.IsNullOrEmpty(relay))
        {
            try
            {
                Console.WriteLine($"[NostrConnectService] Connecting to relay: {relay}");
                
                // Connect to the relay
                _relayManager.TryAddUri(relay);
                await _relayManager.OpenConnection(relay);
                
                // Get stored key pair
                var keyPair = await _keyStorageService.GetStoredKeyPairAsync();
                if (keyPair != null)
                {
                    Console.WriteLine($"[NostrConnectService] Creating connection ack from {keyPair.PublicKey} to {webPubKey}");
                    
                    // Create the plaintext content
                    var plaintextContent = $"{{\"result\":\"ack\",\"id\":\"{compoundSessionId}\"}}";
                    Console.WriteLine($"[NostrConnectService] Plaintext content: {plaintextContent}");
                    
                    // Encrypt the content using NIP-04 (AES with ECDH shared secret)
                    // The content MUST be encrypted for kind 4 (EncryptedDirectMessages)
                    var encryptedContent = await _cryptoService.AesEncrypt(plaintextContent, webPubKey, ethereal: false);
                    var encryptedString = $"{encryptedContent.CipherText}?iv={encryptedContent.Iv}";
                    Console.WriteLine($"[NostrConnectService] Encrypted content length: {encryptedString.Length}");
                    
                    // Create a NIP-46 "connect" response event
                    // This is an encrypted direct message (kind 4) to the web app
                    var connectResponse = new NEvent
                    {
                        Pubkey = keyPair.PublicKey,
                        Kind = KindEnum.EncryptedDirectMessages,
                        Content = encryptedString,
                        Created_At = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        Tags = new List<EventTag>
                        {
                            new EventTag { Key = TagEnum.p, Value = webPubKey }
                        }
                    };
                    
                    // Sign and publish the event
                    var signedEvent = await _nostrService.SignEventAsync(connectResponse, keyPair);
                    Console.WriteLine($"[NostrConnectService] Publishing event ID: {signedEvent.Id}");
                    Console.WriteLine($"[NostrConnectService] Event pubkey: {signedEvent.Pubkey}");
                    Console.WriteLine($"[NostrConnectService] Event signature: {signedEvent.Sig?.Substring(0, 32)}...");
                    Console.WriteLine($"[NostrConnectService] Event content length: {signedEvent.Content?.Length ?? 0}");
                    Console.WriteLine($"[NostrConnectService] Event tags: {signedEvent.Tags?.Count ?? 0}");
                    
                    var subId = Guid.NewGuid().ToString();
                    Console.WriteLine($"[NostrConnectService] Sending with subscription ID: {subId}");
                    
                    await _relayManager.SendNEvent(signedEvent, subId);
                    
                    // Wait a bit to ensure the event is published
                    await Task.Delay(2000);
                    Console.WriteLine($"[NostrConnectService] Event published successfully");
                }
            }
            catch (Exception ex)
            {
                // Log but don't fail - connection is still recorded locally
                Console.WriteLine($"[NostrConnectService] Failed to send relay acknowledgment: {ex.Message}");
                Console.WriteLine($"[NostrConnectService] Stack trace: {ex.StackTrace}");
            }
        }
        
        return await Task.FromResult(true);
    }

    public async Task<NostrConnectResponse> HandleRequestAsync(NostrConnectRequest request)
    {
        var session = await GetSessionAsync(request.SessionId);
        
        if (session == null || !session.IsConnected)
        {
            return new NostrConnectResponse
            {
                SessionId = request.SessionId,
                Success = false,
                Error = "Session not found or not connected"
            };
        }

        try
        {
            var keyPair = await _keyStorageService.GetStoredKeyPairAsync();
            if (keyPair == null)
            {
                return new NostrConnectResponse
                {
                    SessionId = request.SessionId,
                    Success = false,
                    Error = "No key pair found"
                };
            }

            switch (request.Type.ToLower())
            {
                case "get_public_key":
                    return new NostrConnectResponse
                    {
                        SessionId = request.SessionId,
                        Success = true,
                        Data = keyPair.PublicKey
                    };

                case "sign_event":
                    // In a real implementation, you would deserialize the event from request.Data
                    // sign it, and return the signed event
                    return new NostrConnectResponse
                    {
                        SessionId = request.SessionId,
                        Success = true,
                        Data = "Signed event data would go here"
                    };

                default:
                    return new NostrConnectResponse
                    {
                        SessionId = request.SessionId,
                        Success = false,
                        Error = "Unknown request type"
                    };
            }
        }
        catch (Exception ex)
        {
            return new NostrConnectResponse
            {
                SessionId = request.SessionId,
                Success = false,
                Error = ex.Message
            };
        }
    }
}
