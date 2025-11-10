namespace NostrConnect.Shared.Models;

public class NostrConnectSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public string WebEphemeralPubKey { get; set; } = string.Empty;
    public string AppPubKey { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConnectedAt { get; set; }
}
