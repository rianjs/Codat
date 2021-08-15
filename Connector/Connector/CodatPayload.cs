using System;

namespace Connector
{
    public record CodatPayload
    {
        public Guid CodatId { get; init; }
        public string Kind { get; init; }
        public DateTimeOffset Timestamp { get; } = DateTimeOffset.Now;
        public byte[] GzipJson { get; init; }
        public TimeSpan Duration { get; init; }
        public int PageCount { get; init; }
        public string Size => GzipJson.Length.ToString("N0");

        public override string ToString()
            => $"{Kind} ({Size} bytes) downloaded in {Duration.TotalMilliseconds:N0}ms, but may have been compressed by the remote server";
    }
}