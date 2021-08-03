using System;
using System.Collections.Generic;

namespace Connector.CodatTypes
{
    internal class BillPayment
    {
        public string Id { get; init; }
        public SupplierRef SupplierRef { get; init; }
        public AccountRef AccountRef { get; init; }
        public decimal TotalAmount { get; init; }
        public string Currency { get; init; }
        public decimal CurrencyRate { get; init; }
        public DateTime Date { get; init; }
        public List<Line> Lines { get; init; }
        public DateTime ModifiedDate { get; init; }
        public DateTime SourceModifiedDate { get; init; }
        public string Reference { get; init; }
        public string Note { get; init; }
    }

    internal class BillPaymentContainer : IPaginated<BillPayment>
    {
        public List<BillPayment> Results { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalResults { get; init; }
        public Links Links { get; init; }
    }
}