using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Connector.CodatTypes
{
    internal class BankAccount
    {
        public string Id { get; init; }
        public string AccountName { get; init; }
        public string Currency { get; init; }
        public decimal Balance { get; init; }
        public decimal AvailableBalance { get; init; }
        public DateTime ModifiedDate { get; init; }
        public DateTime SourceModifiedDate { get; init; }
        public string NominalCode { get; init; }
    }

    internal class BankAccountContainer : IPaginated<BankAccount>
    {
        public List<BankAccount> Results { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalResults { get; init; }
        
        [JsonProperty("_links")]
        public Links Links { get; init; }
    }


}