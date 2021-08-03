namespace Connector.CodatTypes
{
    internal class Allocation
    {
        public string Currency { get; init; }
        public decimal CurrencyRate { get; init; }
        public decimal TotalAmount { get; init; }
    }
}