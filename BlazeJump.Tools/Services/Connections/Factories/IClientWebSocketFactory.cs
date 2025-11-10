namespace BlazeJump.Tools.Services.Connections.Factories;

/// <summary>
/// Factory interface for creating ClientWebSocket wrapper instances.
/// </summary>
public interface IClientWebSocketFactory
{
    /// <summary>
    /// Creates a new instance of IClientWebSocketWrapper.
    /// </summary>
    /// <returns>A new IClientWebSocketWrapper instance.</returns>
    IClientWebSocketWrapper Create();
}