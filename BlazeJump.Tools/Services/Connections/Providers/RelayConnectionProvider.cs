using BlazeJump.Tools.Services.Connections.Factories;

namespace BlazeJump.Tools.Services.Connections.Providers;

/// <summary>
/// Provider implementation for creating relay connection instances.
/// </summary>
public class RelayConnectionProvider : IRelayConnectionProvider
{
    /// <summary>
    /// Creates a new relay connection for the specified URI.
    /// </summary>
    /// <param name="uri">The URI of the relay server.</param>
    /// <returns>A new IRelayConnection instance.</returns>
    public IRelayConnection CreateRelayConnection(string uri)
    {
        return new RelayConnection(new ClientWebSocketFactory(), uri);
    }
}