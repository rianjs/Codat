using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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
            var client = new CodatClient(codatHttpClient);
            
            // CompanyId = 36eadd34-0006-4db1-9cd5-3cb9dd38618a
            // ConnectionId = 783153fc-660d-477f-8016-75dba9b2ed1a

            var companyId = Guid.Parse("36eadd34-0006-4db1-9cd5-3cb9dd38618a");

            // var bankTransactions = await client.GetBankTransactionsAsync(atwellGuid, ct);   // TODO: Fix
            
            var allPayloads = await client.GetPayloadsAsync(companyId, ct);
            var contentSize = allPayloads.Sum(r => r.Json.Length).ToString("N0");
            
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
    }
}
