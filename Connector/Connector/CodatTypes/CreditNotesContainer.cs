using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Connector.CodatTypes
{
    internal class CreditNoteLineItem
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

    internal class CreditNotePaymentAllocation
    {
        public string Id { get; init; }
        public decimal TotalAmount { get; init; }
        public string Currency { get; init; }
        public decimal CurrencyRate { get; init; }
        public DateTime Date { get; init; }
    }

    internal class CreditNotes
    {
        public string Id { get; init; }
        public string CreditNoteNumber { get; init; }
        public CustomerRef CustomerRef { get; init; }
        public List<object> WithholdingTax { get; init; }
        public decimal TotalAmount { get; init; }
        public decimal TotalDiscount { get; init; }
        public decimal SubTotal { get; init; }
        public decimal TotalTaxAmount { get; init; }
        public decimal DiscountPercentage { get; init; }
        public decimal RemainingCredit { get; init; }
        public string Status { get; init; }
        public DateTime IssueDate { get; init; }
        public string Currency { get; init; }
        public decimal CurrencyRate { get; init; }
        public List<CreditNoteLineItem> LineItems { get; init; }
        public List<CreditNotePaymentAllocation> PaymentAllocations { get; init; }
        public DateTime ModifiedDate { get; init; }
        public DateTime SourceModifiedDate { get; init; }
        public string Note { get; init; }
    }

    internal class CreditNotesContainer : IPaginated<CreditNotes>
    {
        public List<CreditNotes> Results { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalResults { get; init; }
        
        [JsonProperty("_links")]
        public Links Links { get; init; }
    }
}