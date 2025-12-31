/// <summary>
/// Represents the various states of a NostrConnect session handshake and lifecycle.
/// </summary>
public enum SessionStatusEnum
{
    /// <summary>
    /// Initial state - no connection attempt made yet
    /// </summary>
    Idle,
    
    /// <summary>
    /// Client has generated QR code and is waiting for device to scan
    /// </summary>
    AwaitingScan,
    
    /// <summary>
    /// Device has scanned QR code and received client info
    /// </summary>
    QRScanned,
    
    /// <summary>
    /// Device has sent initial response to client
    /// </summary>
    ResponseSent,
    
    /// <summary>
    /// Client has received pong - handshake complete
    /// </summary>
    Connected,
    
    /// <summary>
    /// Connection has been terminated
    /// </summary>
    Disconnected,
    
    /// <summary>
    /// Error occurred during handshake or connection
    /// </summary>
    Error
}
