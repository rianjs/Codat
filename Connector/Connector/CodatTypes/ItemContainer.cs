using System;
using System.Collections.Generic;

namespace Connector.CodatTypes
{
    internal class InvoiceItem
    {
        public string Description { get; set; }
        public AccountRef AccountRef { get; set; }
        public TaxRateRef TaxRateRef { get; set; }
        public decimal? UnitPrice { get; set; }
    }

    internal class BillItem
    {
        public decimal UnitPrice { get; set; }
        public AccountRef AccountRef { get; set; }
        public string Description { get; set; }
    }

    internal class Item
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime SourceModifiedDate { get; set; }
        public string ItemStatus { get; set; }
        public string Type { get; set; }
        public bool IsBillItem { get; set; }
        public bool IsInvoiceItem { get; set; }
        public InvoiceItem InvoiceItem { get; set; }
        public BillItem BillItem { get; set; }
    }

    internal class ItemContainer : IPaginated<Item>
    {
        public List<Item> Results { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalResults { get; set; }
        public Links Links { get; set; }
    }
}