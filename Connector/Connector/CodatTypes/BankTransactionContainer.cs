using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Connector.CodatTypes
{
    internal class BankTransaction
    {
        public string Id { get; init; }
        public DateTime Date { get; init; }
        public string Description { get; init; }
        public bool Reconciled { get; init; }
        public decimal Amount { get; init; }
        public decimal Balance { get; init; }
        public string TransactionType { get; init; }
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