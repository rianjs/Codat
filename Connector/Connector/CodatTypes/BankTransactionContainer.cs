using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Connector.CodatTypes
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    internal class BankTransaction
    {
        public string Id { get; init; }
        public string AccountName { get; init; }
        public string NominalCode { get; init; }
        public string Currency { get; init; }
        public decimal Balance { get; init; }
        public decimal AvailableBalance { get; init; }
        public DateTime ModifiedDate { get; init; }
        public DateTime SourceModifiedDate { get; init; }
    }

    internal class BankTransactionContainer : IPaginated<BankTransaction>
    {
        public List<BankTransaction> Results { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalResults { get; init; }
        
        [JsonProperty("_links")]
        public Links Links { get; init; }
    }
}