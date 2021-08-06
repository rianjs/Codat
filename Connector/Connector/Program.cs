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
            var client = new CodatClient(codatHttpClient, jsonSettings);
            
            // CompanyId = 36eadd34-0006-4db1-9cd5-3cb9dd38618a
            // ConnectionId = 783153fc-660d-477f-8016-75dba9b2ed1a

            var companyId = Guid.Parse("36eadd34-0006-4db1-9cd5-3cb9dd38618a");
            var initialized = client.GetConnectionIdAsync(companyId, ct);

            // var bankTransactions = await client.GetBankTransactionsAsync(atwellGuid, ct);   // TODO: Fix
            // var balanceSheet = await client.GetBalanceSheetAsync(companyId, ct);
            var allPayloads = await client.GetPayloadsAsync(companyId, ct);
            var contentSize = allPayloads.Sum(r => r.Json.Length).ToString("N0");
            var serialized = JsonConvert.SerializeObject(allPayloads, jsonSettings);
            var resultsPath = Path.Combine(GetScratchDirectory(), "results.json");
            File.WriteAllText(resultsPath, serialized);
            
            Console.WriteLine(contentSize);
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
    }
}
