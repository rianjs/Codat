using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Connector.CodatTypes
{
    internal class JournalLine
    {
        public string Description { get; init; }
        public double NetAmount { get; init; }
        public string Currency { get; init; }
        public AccountRef AccountRef { get; init; }
    }

    internal class RecordRef
    {
        public string Id { get; init; }
        public string DataType { get; init; }
    }

    internal class JournalEntry
    {
        public string Id { get; init; }
        public DateTime PostedOn { get; init; }
        public DateTime CreatedOn { get; init; }
        public List<JournalLine> JournalLines { get; init; }
        public DateTime ModifiedDate { get; init; }
        public DateTime SourceModifiedDate { get; init; }
        public RecordRef RecordRef { get; init; }
    }

    internal class JournalEntryContainer : IPaginated<JournalEntry>
    {
        public List<JournalEntry> Results { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalResults { get; init; }
        
        [JsonProperty("_links")]
        public Links Links { get; init; }
    }
}