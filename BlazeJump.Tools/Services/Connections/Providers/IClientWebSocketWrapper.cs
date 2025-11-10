using System.Net.WebSockets;

/// <summary>
/// Wrapper interface for ClientWebSocket to enable testability and dependency injection.
/// </summary>
public interface IClientWebSocketWrapper
{
    /// <summary>
    /// Connects to a WebSocket server as an asynchronous operation.
    /// </summary>
    /// <param name="uri">The URI of the WebSocket server to connect to.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ConnectAsync(Uri uri, CancellationToken cancellationToken);
    
    /// <summary>
    /// Sends data over the WebSocket connection as an asynchronous operation.
    /// </summary>
    /// <param name="buffer">The buffer containing the message to be sent.</param>
    /// <param name="messageType">Indicates whether the application is sending a binary or text message.</param>
    /// <param name="endOfMessage">Indicates whether this is the final message.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken);
    
    /// <summary>
    /// Receives data from the WebSocket connection as an asynchronous operation.
    /// </summary>
    /// <param name="buffer">The buffer to receive the response.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation with the receive result.</returns>
    Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken);
    
    /// <summary>
    /// Closes the WebSocket connection as an asynchronous operation.
    /// </summary>
    /// <param name="closeStatus">The WebSocket close status.</param>
    /// <param name="statusDescription">A description of the close status.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken);
    
    /// <summary>
    /// Releases all resources used by the WebSocket.
    /// </summary>
    void Dispose();
    
    /// <summary>
    /// Gets the current state of the WebSocket connection.
    /// </summary>
    WebSocketState State { get; }
}