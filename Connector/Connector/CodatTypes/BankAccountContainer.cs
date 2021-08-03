using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Connector.CodatTypes
{
    internal class BankAccount
    {
        public string Id { get; init; }
        public string NominalCode { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }
        public string FullyQualifiedCategory { get; init; }
        public string FullyQualifiedName { get; init; }
        public string Currency { get; init; }
        public decimal CurrentBalance { get; init; }
        public string Type { get; init; }
        public string Status { get; init; }
        public bool IsBankAccount { get; init; }
        public DateTime ModifiedDate { get; init; }
        public DateTime SourceModifiedDate { get; init; }
        public List<object> ValidDatatypeLinks { get; init; }
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