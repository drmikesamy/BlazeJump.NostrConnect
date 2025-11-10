namespace BlazeJump.Tools.Enums
{
    /// <summary>
    /// Nostr event kind types as defined in the Nostr protocol specification.
    /// </summary>
    public enum KindEnum
    {
        /// <summary>
        /// User metadata event (kind 0).
        /// </summary>
        Metadata = 0,
        
        /// <summary>
        /// Text note event (kind 1).
        /// </summary>
        Text = 1,
        
        /// <summary>
        /// Relay recommendation event (kind 2).
        /// </summary>
        RecommendRelay = 2,
        
        /// <summary>
        /// Contact list event (kind 3).
        /// </summary>
        Contacts = 3,
        
        /// <summary>
        /// Encrypted direct message event (kind 4).
        /// </summary>
        EncryptedDirectMessages = 4,
        
        /// <summary>
        /// Event deletion request (kind 5).
        /// </summary>
        EventDeletion = 5,
        
        /// <summary>
        /// Repost event (kind 6).
        /// </summary>
        Repost = 6,
        
        /// <summary>
        /// Reaction event (kind 7).
        /// </summary>
        Reaction = 7,
        
        /// <summary>
        /// Channel creation event (kind 40).
        /// </summary>
        ChannelCreation = 40,
        
        /// <summary>
        /// Channel metadata event (kind 41).
        /// </summary>
        ChannelMetadata = 41,
        
        /// <summary>
        /// Channel message event (kind 42).
        /// </summary>
        ChannelMessage = 42,
        
        /// <summary>
        /// Channel hide message event (kind 43).
        /// </summary>
        ChannelHideMessage = 43,
        
        /// <summary>
        /// Channel mute user event (kind 44).
        /// </summary>
        ChannelMuteUser = 44,
        
        /// <summary>
        /// Reserved for public chat events (kind 45).
        /// </summary>
        PublicChatReserved = 45,
        
        /// <summary>
        /// Reserved for replaceable events (kind 10000).
        /// </summary>
        ReplaceableEventsReserved = 10000,
        
        /// <summary>
        /// Relay list metadata event (kind 10002).
        /// </summary>
        RelayListMetadata = 10002,
        
        /// <summary>
        /// Reserved for ephemeral events (kind 20000).
        /// </summary>
        EphemeralEventsReserved = 20000,
        
        /// <summary>
        /// Nostr Connect protocol event (kind 24133).
        /// </summary>
        NostrConnect = 24133,
        
        /// <summary>
        /// Communities event (kind 34550).
        /// </summary>
        Communities = 34550
    }
}
