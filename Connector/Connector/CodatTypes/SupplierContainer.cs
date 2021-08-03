using System;
using System.Collections.Generic;

namespace Connector.CodatTypes
{
    internal class Supplier
    {
        public string Id { get; init; }
        public string SupplierName { get; init; }
        public List<Address> Addresses { get; init; }
        public string Status { get; init; }
        public DateTime ModifiedDate { get; init; }
        public DateTime SourceModifiedDate { get; init; }
        public string DefaultCurrency { get; init; }
        public string Phone { get; init; }
        public string ContactName { get; init; }
    }

    internal class SupplierContainer : IPaginated<Supplier>
    {
        public List<Supplier> Results { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalResults { get; init; }
        public Links Links { get; init; }
    }
}