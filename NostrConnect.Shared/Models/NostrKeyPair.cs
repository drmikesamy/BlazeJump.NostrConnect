namespace NostrConnect.Shared.Models;

public class NostrKeyPair
{
    public string PublicKey { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
    public bool IsEphemeral { get; set; }
}
