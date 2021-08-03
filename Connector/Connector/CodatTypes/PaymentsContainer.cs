using System.Collections.Generic;

namespace Connector.CodatTypes
{
    internal class Link
    {
        public string Type { get; init; }
        public string Id { get; init; }
        public decimal Amount { get; init; }
        public decimal CurrencyRate { get; init; }
    }

    internal class Line
    {
        public decimal Amount { get; init; }
        public List<Link> Links { get; init; }
    }

    internal class PaymentsContainer : IPaginated<Payment>
    {
        public List<Payment> Results { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalResults { get; init; }
        public Links Links { get; init; }
    }
}