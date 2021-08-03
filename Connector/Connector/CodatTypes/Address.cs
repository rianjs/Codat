namespace Connector.CodatTypes
{
    internal class Address
    {
        public string Type { get; init; }
        public string Line1 { get; init; }
        public string Line2 { get; init; }
        public string City { get; init; }
        public string Region { get; init; }
        public string PostalCode { get; init; }
        public string Country { get; init; }
    }
}