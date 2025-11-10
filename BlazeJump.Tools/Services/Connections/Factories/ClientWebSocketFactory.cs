namespace BlazeJump.Tools.Services.Connections.Factories;

/// <summary>
/// Factory implementation for creating ClientWebSocket wrapper instances.
/// </summary>
public class ClientWebSocketFactory : IClientWebSocketFactory
{
    /// <summary>
    /// Creates a new instance of IClientWebSocketWrapper.
    /// </summary>
    /// <returns>A new IClientWebSocketWrapper instance.</returns>
    public IClientWebSocketWrapper Create()
    {
        return new ClientWebSocketWrapper();
    }
}