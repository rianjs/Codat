using System.Collections.Generic;
using Newtonsoft.Json;

namespace Connector.CodatTypes
{
    internal interface IPaginated<T>
    {
        public Links Links { get; }
        public List<T> Results { get; }
    }

    internal class Links
    {
        public Href Current { get; init; }
        public Href Self { get; init; }
        public Href Next { get; init; }
    }
    
    internal class Href
    {
        [JsonProperty("Href")]
        public string Link { get; init; }
    }
}