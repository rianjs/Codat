using System;

namespace Connector.CodatTypes
{
    internal record AccountingDataConnection
    {
        public Guid Id { get; init; }
        public Guid IntegrationId { get; init; }
        public Guid SourceId { get; init; }
        public string PlatformName { get; init; }
        public string LinkUrl { get; init; }
        public string Status { get; init; }
        public DateTime LastSync { get; init; }
        public DateTime Created { get; init; }
        public string SourceType { get; init; }
    }
}