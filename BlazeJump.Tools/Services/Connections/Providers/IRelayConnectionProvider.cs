namespace BlazeJump.Tools.Services.Connections.Providers;

/// <summary>
/// Provider interface for creating relay connection instances.
/// </summary>
public interface IRelayConnectionProvider
{
    /// <summary>
    /// Creates a new relay connection for the specified URI.
    /// </summary>
    /// <param name="uri">The URI of the relay server.</param>
    /// <returns>A new IRelayConnection instance.</returns>
    IRelayConnection CreateRelayConnection(string uri);
}