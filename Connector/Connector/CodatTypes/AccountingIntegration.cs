using System;
using System.Collections.Generic;

namespace Connector.CodatTypes
{
    internal class DataConnection
    {
        public string Id { get; init; }
        public string IntegrationId { get; init; }
        public string SourceId { get; init; }
        public string PlatformName { get; init; }
        public string LinkUrl { get; init; }
        public string Status { get; init; }
        public DateTime LastSync { get; init; }
        public DateTime Created { get; init; }
        public string SourceType { get; init; }
    }

    internal class AccountingIntegration
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public string Platform { get; init; }
        public string Redirect { get; init; }
        public DateTime LastSync { get; init; }
        public List<DataConnection> DataConnections { get; init; }
        public DateTime Created { get; init; }
    }
}