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
            
            // TODO: JsonSettings for casing, default values and empty values
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
            var client = new CodatClient(26, codatHttpClient, jsonSettings, GetLogger<IAccountingDataReader>());
            
            // download the data for client id
            // Persist the data in gzip form to S3

            const string dmitry = "39b788e5-7190-4ffc-917b-4d3fe86b81b8";
            const string rollingVideoGames = "ca545af7-392c-4890-b528-0b38dd37ee95";

            var dmitryTasks = client.GetAllDataAsync(Guid.Parse(dmitry), ct);
            var rollingVideoTasks = client.GetAllDataAsync(Guid.Parse(rollingVideoGames), ct);

            var rollingVideoResults = await rollingVideoTasks;
            WriteResults($"{nameof(rollingVideoGames)}", rollingVideoResults, jsonSettings);

            var dmitryResults = await dmitryTasks;
            WriteResults($"{nameof(dmitry)}", dmitryResults, jsonSettings);

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
    }
}
