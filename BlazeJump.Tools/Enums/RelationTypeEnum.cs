namespace BlazeJump.Tools.Enums
{
	/// <summary>
	/// Types of relationships between entities in the application (search queries, subscriptions, events, users).
	/// </summary>
	[System.Flags]
    public enum RelationTypeEnum
	{
		/// <summary>
		/// Relation mapping a search query to subscription IDs.
		/// </summary>
		SearchToSubscriptionId = 10,
		
		/// <summary>
		/// Relation mapping a subscription GUID to event IDs.
		/// </summary>
		SubscriptionGuidToIds = 11,
		
		/// <summary>
		/// Relation for root level subscriptions.
		/// </summary>
		RootLevelSubscription = 14,
		
		/// <summary>
		/// Relation mapping an event to its child events (replies).
		/// </summary>
		EventChildren = 15,
		
		/// <summary>
		/// Relation mapping a user to their events.
		/// </summary>
		EventsByUser = 16,
		
		/// <summary>
		/// Relation mapping an event to its author (user).
		/// </summary>
		UserByEvent = 17,
		
		/// <summary>
		/// Relation mapping an event to its direct parent event.
		/// </summary>
		EventParent = 18,
		
		/// <summary>
		/// Relation mapping an event to its root event in a thread.
		/// </summary>
		EventRoot = 19,
		
		/// <summary>
		/// Relation for referenced events.
		/// </summary>
		ReferencedEvent = 20,
	}
}
