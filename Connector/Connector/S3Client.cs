using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace Connector
{
    public class S3Client :
        IAccountingPersistor
    {
        private readonly IAmazonS3 _s3;
        private readonly string _bucket;
        private readonly ILogger<S3Client> _log;

        public S3Client(IAmazonS3 s3, string accountingDataBucket, ILogger<S3Client> log)
        {
            _s3 = s3 ?? throw new ArgumentNullException(nameof(s3));
            _bucket = string.IsNullOrWhiteSpace(accountingDataBucket) ? throw new ArgumentNullException(nameof(accountingDataBucket)) : accountingDataBucket;
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task SavePayloadAsync(CodatPayload payload, CancellationToken ct)
        {
            var name = GetName(payload);
            _log.LogInformation($"Writing payload {name} ({payload.Size} bytes)");
            
            var timer = Stopwatch.StartNew();
            PutObjectResponse resp;
            using (var ms = new MemoryStream(payload.GzipJson))
            {
                var putReq = new PutObjectRequest
                {
                    BucketName = _bucket,
                    Key = name,
                    InputStream = ms,
                };
                resp = await _s3.PutObjectAsync(putReq, ct);
            }
            timer.Stop();

            if (!resp.HttpStatusCode.IsSuccessStatusCode())
            {
                _log.LogError($"{name} was not saved successfully. Status code = {resp.HttpStatusCode}, Elapsed = {timer.ElapsedMilliseconds:N0}ms");
                return;
            }
            
            _log.LogInformation($"{name} ({payload.Size} bytes) written in {timer.ElapsedMilliseconds:N0}ms");
        }

        private string GetName(CodatPayload payload)
            => $"{payload.CodatId}-{payload.Kind}.json.gz";
    }
}