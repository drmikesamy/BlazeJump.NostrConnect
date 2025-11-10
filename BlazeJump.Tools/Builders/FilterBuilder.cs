using BlazeJump.Tools.Enums;
using BlazeJump.Tools.Models;

namespace BlazeJump.Tools.Builders
{
	/// <summary>
	/// Builder class for constructing Nostr event filters using a fluent API.
	/// </summary>
	public class FilterBuilder
	{
		private List<Filter> _filters = new List<Filter>();
		private Filter _filter = new Filter();

		/// <summary>
		/// Adds a new filter to the builder.
		/// </summary>
		/// <returns>The current FilterBuilder instance for method chaining.</returns>
		public FilterBuilder AddFilter()
		{
			_filter = new Filter();
			_filters.Add(_filter);
			return this;
		}        /// <summary>
        /// Adds an event kind to the current filter.
        /// </summary>
        /// <param name="kind">The event kind to add.</param>
        /// <returns>The current FilterBuilder instance for method chaining.</returns>
        public FilterBuilder AddKind(KindEnum kind)
        {
            if (_filter.Kinds == null)
            {
                _filter.Kinds = new List<int>();
            }

            _filter.Kinds.Add((int)kind);
            return this;
        }

        /// <summary>
        /// Sets the start date for events in the current filter.
        /// </summary>
        /// <param name="fromDate">The starting date for the filter.</param>
        /// <returns>The current FilterBuilder instance for method chaining.</returns>
        public FilterBuilder WithFromDate(DateTime fromDate)
        {
            _filter.Since = fromDate;
            return this;
        }

        /// <summary>
        /// Sets the end date for events in the current filter.
        /// </summary>
        /// <param name="toDate">The ending date for the filter.</param>
        /// <returns>The current FilterBuilder instance for method chaining.</returns>
        public FilterBuilder WithToDate(DateTime toDate)
        {
            _filter.Until = toDate;
            return this;
        }

        /// <summary>
        /// Sets the maximum number of events to return.
        /// </summary>
        /// <param name="limit">The maximum number of events.</param>
        /// <returns>The current FilterBuilder instance for method chaining.</returns>
        public FilterBuilder WithLimit(int limit)
        {
            _filter.Limit = limit;
            return this;
        }

        /// <summary>
        /// Adds a tagged event ID to the current filter.
        /// </summary>
        /// <param name="taggedEventId">The event ID to add to the tag filter.</param>
        /// <returns>The current FilterBuilder instance for method chaining.</returns>
        public FilterBuilder AddTaggedEventId(string taggedEventId)
        {
            if (_filter.TaggedEventIds == null)
            {
                _filter.TaggedEventIds = new List<string>();
            }

            _filter.TaggedEventIds.Add(taggedEventId);
            return this;
        }

        /// <summary>
        /// Adds multiple tagged event IDs to the current filter.
        /// </summary>
        /// <param name="taggedEventIds">The list of event IDs to add to the tag filter.</param>
        /// <returns>The current FilterBuilder instance for method chaining.</returns>
        public FilterBuilder AddTaggedEventIds(List<string> taggedEventIds)
        {
            if (_filter.TaggedEventIds == null)
            {
                _filter.TaggedEventIds = new List<string>();
            }

            _filter.TaggedEventIds.AddRange(taggedEventIds);
            return this;
        }
        
        /// <summary>
        /// Adds multiple tagged keywords to the current filter.
        /// </summary>
        /// <param name="taggedKeywords">The list of keywords to add to the tag filter.</param>
        /// <returns>The current FilterBuilder instance for method chaining.</returns>
        public FilterBuilder AddTaggedKeywords(List<string> taggedKeywords)
        {
            if (_filter.TaggedKeywords == null)
            {
                _filter.TaggedKeywords = new List<string>();
            }

            _filter.TaggedKeywords.AddRange(taggedKeywords);
            return this;
        }
        
        /// <summary>
        /// Adds a tagged keyword to the current filter.
        /// </summary>
        /// <param name="taggedKeyword">The keyword to add to the tag filter.</param>
        /// <returns>The current FilterBuilder instance for method chaining.</returns>
        public FilterBuilder AddTaggedKeyword(string taggedKeyword)
        {
            if (_filter.TaggedKeywords == null)
            {
                _filter.TaggedKeywords = new List<string>();
            }

            _filter.TaggedKeywords.Add(taggedKeyword);
            return this;
        }

        /// <summary>
        /// Adds an event ID to the current filter.
        /// </summary>
        /// <param name="eventId">The event ID to filter by.</param>
        /// <returns>The current FilterBuilder instance for method chaining.</returns>
        public FilterBuilder AddEventId(string eventId)
        {
            if (_filter.EventIds == null)
            {
                _filter.EventIds = new List<string>();
            }

            _filter.EventIds.Add(eventId);
            return this;
        }

        /// <summary>
        /// Adds multiple event IDs to the current filter.
        /// </summary>
        /// <param name="eventIds">The list of event IDs to filter by.</param>
        /// <returns>The current FilterBuilder instance for method chaining.</returns>
        public FilterBuilder AddEventIds(List<string> eventIds)
        {
            if (_filter.EventIds == null)
            {
                _filter.EventIds = new List<string>();
            }

            _filter.EventIds.AddRange(eventIds);
            return this;
        }

        /// <summary>
        /// Adds an author public key to the current filter.
        /// </summary>
        /// <param name="author">The author public key to filter by.</param>
        /// <returns>The current FilterBuilder instance for method chaining.</returns>
        public FilterBuilder AddAuthor(string author)
        {
            if (_filter.Authors == null)
            {
                _filter.Authors = new List<string>();
            }

            _filter.Authors.Add(author);
            return this;
        }

        /// <summary>
        /// Adds multiple author public keys to the current filter.
        /// </summary>
        /// <param name="authors">The list of author public keys to filter by.</param>
        /// <returns>The current FilterBuilder instance for method chaining.</returns>
        public FilterBuilder AddAuthors(List<string> authors)
        {
            if (_filter.Authors == null)
            {
                _filter.Authors = new List<string>();
            }

            _filter.Authors.AddRange(authors);
            return this;
        }
        
        /// <summary>
        /// Adds a search query to the current filter.
        /// </summary>
        /// <param name="query">The search query string.</param>
        /// <returns>The current FilterBuilder instance for method chaining.</returns>
        public FilterBuilder AddSearch(string query)
        {
            _filter.Search = query;
            return this;
        }

        /// <summary>
        /// Builds and returns the list of filters, applying default date ranges and removing empty filters.
        /// </summary>
        /// <returns>A list of Filter objects.</returns>
        public List<Filter> Build()
        {
            _filters.Where(f => f.Since == null).ToList().ForEach(f => f.Since = DateTime.Now.AddYears(-20));
            _filters.Where(f => f.Until == null).ToList().ForEach(f => f.Until = DateTime.Now.AddDays(1));
            foreach (var filter in _filters.ToList())
            {
                if ((filter.Authors == null || filter.Authors.Count == 0)
                    && (filter.Search == null)
                    && (filter.TaggedEventIds == null || filter.TaggedEventIds.Count == 0)
                    && (filter.TaggedKeywords == null || filter.TaggedKeywords.Count == 0)
                    && (filter.Kinds == null || filter.Kinds.Count == 0)
                    && (filter.EventIds == null || filter.EventIds.Count == 0))
                {
                    _filters.Remove(filter);
                }
            }
            return _filters;
        }
    }
}