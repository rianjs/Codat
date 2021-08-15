using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Connector
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            var ct = cts.Token;
            
            var prodTokenPath = Path.Combine(GetScratchDirectory(), "prod.secret");
            var prodToken = File.ReadAllText(prodTokenPath).Trim();
            
            var jsonSettings = GetJsonSerializerSettings();
            var compressingRefreshingDnsHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromSeconds(120),
                AutomaticDecompression = DecompressionMethods.All,
            };
            var codatHttpClient = new HttpClient(compressingRefreshingDnsHandler)
            {
                BaseAddress = new Uri("https://api.codat.io"),
                DefaultRequestHeaders = { Authorization = GetBasicAuthHeader(prodToken),},
            };
            var accountingReader = new CodatClient(26, codatHttpClient, jsonSettings, GetLogger<IAccountingDataReader>());
            
            var (credentials, regionEndpoint) = GetAwsConfig("rianjs");
            var s3 = new AmazonS3Client(credentials, regionEndpoint);
            var accountingWriter = new S3AccountingWriter(s3, "monit-codat", GetLogger<IAccountingWriter>());
            
            var importer = new CodatS3AccountingImporter(accountingReader, accountingWriter, GetLogger<IAccountingImporter>());
            
            const string dmitry = "39b788e5-7190-4ffc-917b-4d3fe86b81b8";
            const string rollingVideoGames = "ca545af7-392c-4890-b528-0b38dd37ee95";

            // await importer.ImportDataAsync(Guid.Parse(rollingVideoGames), ct);
            await importer.ImportDataAsync(Guid.Parse(dmitry), ct);

            // var companyId = Guid.Parse(rollingVideoGames);
            // var allPayloads = await client.GetPayloadsAsync(companyId, ct);
            // var contentSize = allPayloads.Sum(r => r.Json.Length).ToString("N0");
            // var serialized = JsonConvert.SerializeObject(allPayloads, jsonSettings);
            // var resultsPath = Path.Combine(GetScratchDirectory(), "video-game-lady.json");
            // File.WriteAllText(resultsPath, serialized);
            //
            // Console.WriteLine(contentSize);
        }

        private static void WriteResults(string label, ICollection<CodatPayload> results, JsonSerializerSettings jsonSettings)
        {
            var path = Path.Combine(GetScratchDirectory(), label + ".json");
            var serialized = JsonConvert.SerializeObject(results, jsonSettings);
            Console.WriteLine($"Writing {path} ({results.Sum(r => r.GzipJson.Length):N0} bytes)");
            File.WriteAllText(path, serialized);
        }
        
        private static string GetScratchDirectory()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            const int netCoreNestingLevel = 4;

            for (var i = 0; i < netCoreNestingLevel; i++)
            {
                path = Directory.GetParent(path).FullName;
            }

            return Path.Combine(path, "data");
        }

        private static AuthenticationHeaderValue GetBasicAuthHeader(string base64EncodedApiToken)
            => new("Basic", base64EncodedApiToken);
        
        private static JsonSerializerSettings GetJsonSerializerSettings()
        {
            #if DEBUG
            return GetDebugJsonSerializerSettings();
            #endif
            
            return GetProdJsonSerializerSettings();
        }

        private static JsonSerializerSettings GetDebugJsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DefaultValueHandling = DefaultValueHandling.Include,
                NullValueHandling = NullValueHandling.Include,
                Formatting = Formatting.Indented,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                Converters = new List<JsonConverter> { new StringEnumConverter(), },
            };
        }

        private static JsonSerializerSettings GetProdJsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.None,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                Converters = new List<JsonConverter> { new StringEnumConverter(), },
            };
        }
        
        private static ILogger<T> GetLogger<T>()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                    .AddConsole();
            });

            var logger = loggerFactory.CreateLogger<T>();
            return logger;
        }
        
        public static (AWSCredentials credentials, RegionEndpoint regionEndpoint) GetAwsConfig(string awsProfileName, string region = "us-east-2")
        {
            if (string.IsNullOrWhiteSpace(awsProfileName)) throw new ArgumentNullException(nameof(awsProfileName));
            if (string.IsNullOrWhiteSpace(region)) throw new ArgumentNullException(nameof(region));
            
            var credProfileStoreChain = new CredentialProfileStoreChain();
            if (credProfileStoreChain.TryGetAWSCredentials(awsProfileName, out var awsCredentials))
            {
                return (awsCredentials, RegionEndpoint.GetBySystemName(region));
            }

            throw new ArgumentException($"{awsProfileName} was not a profile available in the credentials store");
        }
    }
}
