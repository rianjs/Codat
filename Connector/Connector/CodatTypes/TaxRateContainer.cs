using System;
using System.Collections.Generic;

namespace Connector.CodatTypes
{
    internal class Component
    {
        public string Name { get; init; }
        public decimal Rate { get; init; }
        public bool IsCompound { get; init; }
    }

    internal class TaxRate
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public string Code { get; init; }
        public decimal EffectiveTaxRate { get; init; }
        public decimal TotalTaxRate { get; init; }
        public List<Component> Components { get; init; }
        public DateTime ModifiedDate { get; init; }
        public DateTime SourceModifiedDate { get; init; }
        public List<object> ValidDatatypeLinks { get; init; }
    }

    internal class TaxRateContainer : IPaginated<TaxRate>
    {
        public List<TaxRate> Results { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalResults { get; init; }
        public Links Links { get; init; }
    }
}