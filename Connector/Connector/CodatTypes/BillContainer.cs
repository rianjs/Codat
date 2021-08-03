using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Connector.CodatTypes
{
    internal class SupplierRef
    {
        public string Id { get; init; }
        public string SupplierName { get; init; }
    }

    internal class BillingLineItem
    {
        public string Description { get; init; }
        public decimal UnitAmount { get; init; }
        public decimal Quantity { get; init; }
        public decimal DiscountAmount { get; init; }
        public decimal SubTotal { get; init; }
        public decimal TaxAmount { get; init; }
        public decimal TotalAmount { get; init; }
        public decimal DiscountPercentage { get; init; }
        public AccountRef AccountRef { get; init; }
        public List<object> TrackingCategoryRefs { get; init; }
        public bool IsDirectCost { get; init; }
    }

    internal class BillPaymentAllocation
    {
        public Payment Payment { get; init; }
        public Allocation Allocation { get; init; }
    }

    internal class Bill
    {
        public string Id { get; init; }
        public string Reference { get; init; }
        public SupplierRef SupplierRef { get; init; }
        public List<object> PurchaseOrderRefs { get; init; }
        public DateTime IssueDate { get; init; }
        public DateTime DueDate { get; init; }
        public string Currency { get; init; }
        public decimal CurrencyRate { get; init; }
        public List<BillingLineItem> LineItems { get; init; }
        public List<object> WithholdingTax { get; init; }
        public string Status { get; init; }
        public decimal SubTotal { get; init; }
        public decimal TaxAmount { get; init; }
        public decimal TotalAmount { get; init; }
        public decimal AmountDue { get; init; }
        public DateTime ModifiedDate { get; init; }
        public DateTime SourceModifiedDate { get; init; }
        public List<BillPaymentAllocation> PaymentAllocations { get; init; }
    }

    internal class BillContainer : IPaginated<Bill>
    {
        public List<Bill> Results { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalResults { get; init; }
        
        [JsonProperty("_links")]
        public Links Links { get; init; }
    }
}