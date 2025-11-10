using BlazeJump.Tools.Enums;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BlazeJump.Tools.Services.Message
{
	/// <summary>
	/// Manages relationships between Nostr events and users.
	/// </summary>
	public class RelationRegister
	{
		/// <summary>
		/// Gets or sets the relationships dictionary.
		/// </summary>
		public ConcurrentDictionary<string, ConcurrentDictionary<RelationTypeEnum, ConcurrentDictionary<string, bool>>> Relationships { get; set; } = new();

		/// <summary>
		/// Adds a relationship between a parent and child event.
		/// </summary>
		/// <param name="parentEventId">The parent event ID.</param>
		/// <param name="relationType">The type of relationship.</param>
		/// <param name="childEventId">The child event ID.</param>
		public void AddRelation(string parentEventId, RelationTypeEnum relationType, string childEventId)
		{
			var relationDict = Relationships.GetOrAdd(parentEventId, _ => new ConcurrentDictionary<RelationTypeEnum, ConcurrentDictionary<string, bool>>());
			var childDict = relationDict.GetOrAdd(relationType, _ => new ConcurrentDictionary<string, bool>());
			childDict.TryAdd(childEventId, true);
		}

		/// <summary>
		/// Tries to get related child events for a parent event.
		/// </summary>
		/// <param name="parentEventId">The parent event ID.</param>
		/// <param name="relationType">The type of relationship.</param>
		/// <param name="childEventIds">The list of child event IDs.</param>
		/// <returns>True if relationships exist; otherwise, false.</returns>
		public bool TryGetRelation(string parentEventId, RelationTypeEnum relationType, out List<string> childEventIds)
		{
			if (Relationships.TryGetValue(parentEventId, out var kindsAndChildEventIds) &&
				kindsAndChildEventIds.TryGetValue(relationType, out var childEventIdDict))
			{
				childEventIds = childEventIdDict.Keys.ToList();
				return true;
			}
			childEventIds = new List<string>();
			return false;
		}

		/// <summary>
		/// Tries to get related child events for multiple parent events.
		/// </summary>
		/// <param name="parentEventIds">The list of parent event IDs.</param>
		/// <param name="relationType">The type of relationship.</param>
		/// <param name="childEventIds">The combined list of child event IDs.</param>
		/// <returns>True if any relationships exist; otherwise, false.</returns>
		public bool TryGetRelations(List<string> parentEventIds, RelationTypeEnum relationType, out List<string> childEventIds)
		{
			childEventIds = new List<string>();
			foreach (var parentEventId in parentEventIds)
			{
				if (Relationships.TryGetValue(parentEventId, out var kindsAndChildEventIds) &&
					kindsAndChildEventIds.TryGetValue(relationType, out var childIds))
				{
					childEventIds.AddRange(childIds.Keys);
				}
			}
			return childEventIds.Count > 0;
		}

		/// <summary>
		/// Checks if a relationship exists for a parent event.
		/// </summary>
		/// <param name="parentEventId">The parent event ID.</param>
		/// <param name="relationType">The type of relationship.</param>
		/// <returns>True if the relationship exists; otherwise, false.</returns>
		public bool RelationExists(string parentEventId, RelationTypeEnum relationType)
		{
			return Relationships.ContainsKey(parentEventId) && Relationships[parentEventId].ContainsKey(relationType);
		}

		/// <summary>
		/// Gets all relationships.
		/// </summary>
		/// <returns>The complete relationships dictionary.</returns>
		public ConcurrentDictionary<string, ConcurrentDictionary<RelationTypeEnum, ConcurrentDictionary<string, bool>>> GetAll()
		{
			return Relationships;
		}
	}
}
