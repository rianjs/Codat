using System;
using System.Collections.Generic;

namespace Connector.CodatTypes
{
    internal class Payment
    {
        public string Id { get; init; }
        public CustomerRef CustomerRef { get; init; }
        public AccountRef AccountRef { get; init; }
        public decimal TotalAmount { get; init; }
        public string Currency { get; init; }
        public decimal CurrencyRate { get; init; }
        public DateTime Date { get; init; }
        public string Note { get; init; }
        public List<Line> Lines { get; init; }
        public DateTime ModifiedDate { get; init; }
        public DateTime SourceModifiedDate { get; init; }
        public string Reference { get; init; }
        public DateTime PaidOnDate { get; init; }
    }
}