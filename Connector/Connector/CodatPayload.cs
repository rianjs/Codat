using System;
using Newtonsoft.Json;

namespace Connector
{
    public record CodatPayload
    {
        public Guid CodatId { get; init; }
        public string Kind { get; init; }
        public DateTimeOffset Timestamp { get; } = DateTimeOffset.Now;
        public string Json { get; init; }
        
        [JsonIgnore]
        public string Size => Json.Length.ToString("N0");
    }
}