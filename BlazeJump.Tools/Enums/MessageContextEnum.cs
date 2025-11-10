namespace BlazeJump.Tools.Enums
{
    /// <summary>
    /// Context types for messages in the application.
    /// </summary>
    public enum MessageContextEnum
    {
        /// <summary>
        /// Message in the context of an event.
        /// </summary>
        Event = 0,
        
        /// <summary>
        /// Message in the context of a user.
        /// </summary>
        User = 1,
        
        /// <summary>
        /// Message that is a reply.
        /// </summary>
        Reply = 2
    }
}
