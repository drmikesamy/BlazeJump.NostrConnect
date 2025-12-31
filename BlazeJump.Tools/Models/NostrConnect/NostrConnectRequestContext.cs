using BlazeJump.Tools.Enums;

namespace BlazeJump.Tools.Models.NostrConnect;

/// <summary>
/// Contains context information for a NostrConnect request to facilitate response handling.
/// </summary>
public class NostrConnectRequestContext
{
    /// <summary>
    /// Gets or sets the session id.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the command that was sent in the request.
    /// </summary>
    public CommandEnum Command { get; set; }

    /// <summary>
    /// Gets or sets the public key the request was sent to.
    /// </summary>
    public string TargetPubkey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the relay used for the request.
    /// </summary>
    public string Relay { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the request was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the original parameters sent with the request.
    /// </summary>
    public string[] Parameters { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets optional metadata for the request.
    /// Can be used to store additional context-specific information.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}
