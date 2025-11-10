namespace NostrConnect.Shared.Models;

public class NostrConnectRequest
{
    public string Type { get; set; } = string.Empty; // "connect", "sign_event", "get_public_key"
    public string SessionId { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
}
