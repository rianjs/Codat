using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Connector.CodatTypes
{
    internal class ItemRef
    {
        public string Id { get; init; }
        public string Name { get; init; }
    }

    internal class InvoiceLineItem
    {
        public string Description { get; init; }
        public decimal UnitAmount { get; init; }
        public decimal Quantity { get; init; }
        public decimal SubTotal { get; init; }
        public decimal TaxAmount { get; init; }
        public decimal TotalAmount { get; init; }
        public TaxRateRef TaxRateRef { get; init; }
        public ItemRef ItemRef { get; init; }
        public List<object> TrackingCategoryRefs { get; init; }
    }

    internal class InvoicePaymentAllocation
    {
        public string Id { get; init; }
        public decimal TotalAmount { get; init; }
        public string Currency { get; init; }
        public decimal CurrencyRate { get; init; }
        public DateTime Date { get; init; }
        public string Note { get; init; }
        public AccountRef AccountRef { get; init; }
    }

    internal class Invoice
    {
        public string Id { get; init; }
        public string InvoiceNumber { get; init; }
        public CustomerRef CustomerRef { get; init; }
        public DateTime IssueDate { get; init; }
        public DateTime DueDate { get; init; }
        public DateTime ModifiedDate { get; init; }
        public DateTime SourceModifiedDate { get; init; }
        public DateTime PaidOnDate { get; init; }
        public string Currency { get; init; }
        public decimal CurrencyRate { get; init; }
        public List<InvoiceLineItem> LineItems { get; init; }
        public List<InvoicePaymentAllocation> PaymentAllocations { get; init; }
        public List<object> WithholdingTax { get; init; }
        public decimal SubTotal { get; init; }
        public decimal TotalTaxAmount { get; init; }
        public decimal TotalAmount { get; init; }
        public decimal AmountDue { get; init; }
        public string Status { get; init; }
        public string Note { get; init; }
    }

    internal class InvoicesContainer : IPaginated<Invoice>
    {
        public List<Invoice> Results { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalResults { get; init; }
        
        [JsonProperty("_links")]
        public Links Links { get; init; }
    }
}