namespace NostrConnect.Shared.Models;

public class NostrConnectResponse
{
    public string SessionId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Data { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}
