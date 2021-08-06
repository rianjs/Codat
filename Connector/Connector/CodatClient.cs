using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Connector.CodatTypes;
using Newtonsoft.Json;

namespace Connector
{
    public class CodatClient
    {
        // 36eadd34-0006-4db1-9cd5-3cb9dd38618a -- atwell enterprises
        private readonly HttpClient _codatClient;
        private readonly JsonSerializerSettings _jsonSettings;
        private readonly Dictionary<Guid, Guid> _companyIdsToConnectionIds;
        private readonly SemaphoreSlim _companyIdsToConnectionIdsLock = new(1, 1);

        public CodatClient(HttpClient codatClient, JsonSerializerSettings jsonSettings)
        {
            _codatClient = codatClient ?? throw new ArgumentNullException(nameof(codatClient));
            _jsonSettings = jsonSettings ?? throw new ArgumentNullException(nameof(jsonSettings));
            _companyIdsToConnectionIds = new Dictionary<Guid, Guid>();
        }

        public async Task<ICollection<CodatPayload>> GetPayloadsAsync(Guid companyId, CancellationToken ct)
        {
            var timer = Stopwatch.StartNew();
            var payloadTasks = new List<Task<CodatPayload>>
            {
                GetBalanceSheetAsync(companyId, ct),
                GetBankAccounts(companyId, ct),
                GetBillsAsync(companyId, ct),
                GetChartOfAccountsAsync(companyId, ct),
                GetCompanyInfoAsync(companyId, ct),
                GetCreditNotesAsync(companyId, ct),
                GetCustomersAsync(companyId, ct),
                // GetBankTransactionsAsync(companyId, ct),
                GetInvoicesAsync(companyId, ct),
                GetPaymentsAsync(companyId, ct),
                GetProfitAndLossAsync(companyId, ct),
                // GetJournalEntriesAsync(companyId, ct),
                GetSuppliersAsync(companyId, ct),
                GetBillPaymentsAsync(companyId, ct),
                GetTaxRatesAsync(companyId, ct),
            };

            await Task.WhenAll(payloadTasks);
            timer.Stop();
            
            Console.WriteLine($"Downloading data took {timer.ElapsedMilliseconds:N0} ms");

            var results = payloadTasks
                .Where(t => t.IsCompletedSuccessfully)
                .Select(t => t.Result)
                .ToList();
            return results;
        }

        public async Task<CodatPayload> GetBalanceSheetAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "BalanceSheet";
            const int periodLength = 30;
            const int periodsToCompare = 12;
            var url = $"/companies/{companyId}/data/financials/balanceSheet?periodLength={periodLength}&periodsToCompare={periodsToCompare}";

            return await DownloadPayloadAsync(companyId, url, kind, ct);
        }

        public async Task<CodatPayload> GetBankAccounts(Guid companyId, CancellationToken ct)
        {
            const string kind = "BankAccounts";
            var connectionId = await GetConnectionIdAsync(companyId, ct);

            var baseUrl = $"/companies/{companyId}/connections/{connectionId}/data/bankAccounts";
            
            // /companies/{companyId}/data/bankAccounts/{accountId}/transactions
            var accountResults = await GetPaginatedResultsAsync<BankAccountContainer, BankAccount>(companyId, kind, baseUrl, ct);
            var bankAccountContainer = JsonConvert.DeserializeObject<BankAccountContainer>(accountResults.Json, _jsonSettings);
            var bankAccounts = bankAccountContainer.Results.Where(r => r.IsBankAccount).ToList();
            bankAccountContainer.Results.Clear();
            bankAccountContainer.Results.AddRange(bankAccounts);
            return new CodatPayload
            {
                CodatId = companyId,
                Kind = kind,
                Json = JsonConvert.SerializeObject(bankAccountContainer, _jsonSettings),
                Duration = accountResults.Duration,
                PageCount = accountResults.PageCount,
            };
        }

        public async Task<CodatPayload> GetBillsAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "Bills";
            
            var baseUrl = $"/companies/{companyId}/data/bills";
            var billContainer = await GetPaginatedResultsAsync<BillContainer, Bill>(companyId, kind, baseUrl, ct);

            return billContainer;
        }

        public async Task<CodatPayload> GetChartOfAccountsAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "ChartOfAccounts";
            var baseUrl = $"/companies/{companyId}/data/accounts";

            var accounts = await GetPaginatedResultsAsync<AccountsContainer, Account>(companyId, kind, baseUrl, ct);
            return accounts;
        }

        public async Task<CodatPayload> GetCompanyInfoAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "CompanyInfo";
            var baseUrl = $"/companies/{companyId}/data/info";

            var companyInfo = await DownloadPayloadAsync(companyId, baseUrl, kind, ct);
            return companyInfo;
        }

        public async Task<CodatPayload> GetCreditNotesAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "CreditNotes";
            var baseUrl = $"/companies/{companyId}/data/creditNotes";

            var creditNotes = await GetPaginatedResultsAsync<CreditNotesContainer, CreditNotes>(companyId, kind, baseUrl, ct);
            return creditNotes;
        }

        public async Task<CodatPayload> GetCustomersAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "Customers";
            var baseUrl = $"/companies/{companyId}/data/customers";

            var customers = await GetPaginatedResultsAsync<CustomersContainer, Customer>(companyId, kind, baseUrl, ct);
            return customers;
        }
        
        public async Task<CodatPayload> GetBankTransactionsAsync(Guid companyId, CancellationToken ct)
        {
            // TODO: Fix this
            const string kind = "BankTransactions";
            var connectionId = await GetConnectionIdAsync(companyId, ct);
            var bankAcctPayloads = await GetBankAccounts(companyId, ct);
            var deserializedBankAccounts = JsonConvert.DeserializeObject<BankAccountContainer>(bankAcctPayloads.Json, _jsonSettings);
            var transactionsQueries = deserializedBankAccounts.Results
                .Select(a => a.Id)
                .Select(accountId =>
                {
                    var baseUrl = $"/companies/{companyId}/connections/{connectionId}/options/bankAccounts/{accountId}/bankTransactions";
                    return GetPaginatedResultsAsync<BankTransactionContainer, BankTransaction>(companyId, kind, baseUrl, ct);
                })
                .ToList();
            await Task.WhenAll(transactionsQueries);

            var consolidated = transactionsQueries
                .Where(q => q.IsCompletedSuccessfully)
                .Select(r => r.Result.Json)
                .Select(j => JsonConvert.DeserializeObject<BankTransactionContainer>(j, _jsonSettings))
                .ToList();
            var serialized = JsonConvert.SerializeObject(consolidated, _jsonSettings);
            return new CodatPayload
            {
                CodatId = companyId,
                Kind = kind,
                Json = serialized,
            };
        }

        public async Task<CodatPayload> GetInvoicesAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "Invoices";
            var baseUrl = $"/companies/{companyId}/data/invoices";

            var customers = await GetPaginatedResultsAsync<InvoicesContainer, Invoice>(companyId, kind, baseUrl, ct);
            return customers;
        }

        public async Task<CodatPayload> GetPaymentsAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "Payments";
            var baseUrl = $"/companies/{companyId}/data/payments";

            var payments = await GetPaginatedResultsAsync<PaymentsContainer, Payment>(companyId, kind, baseUrl, ct);
            return payments;
        }

        public async Task<CodatPayload> GetProfitAndLossAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "ProfitAndLoss";
            var baseUrl = $"/companies/{companyId}/data/financials/profitAndLoss?periodLength=30&periodsToCompare=12";

            var payload = await DownloadPayloadAsync(companyId, baseUrl, kind, ct);
            return payload;
        }

        public async Task<CodatPayload> GetSuppliersAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "Suppliers";
            var baseUrl = $"/companies/{companyId}/data/suppliers";

            var results = await GetPaginatedResultsAsync<SupplierContainer, Supplier>(companyId, kind, baseUrl, ct);
            return results;
        }
        
        public async Task<CodatPayload> GetJournalEntriesAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "JournalEntries";
            var baseUrl = $"/companies/{companyId}/data/journalEntries";

            // TODO: This API appears to be broken
            throw new NotImplementedException();
            // var results = await GetPaginatedResultsAsync<SupplierContainer, Supplier>(companyId, kind, baseUrl, ct);
            // return results;
        }

        public async Task<CodatPayload> GetBillPaymentsAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "BillPayments";
            var baseUrl = $"/companies/{companyId}/data/billPayments";

            var results = await GetPaginatedResultsAsync<BillPaymentContainer, BillPayment>(companyId, kind, baseUrl, ct);
            return results;
        }

        public async Task<CodatPayload> GetTaxRatesAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "TaxRates";
            var baseUrl = $"/companies/{companyId}/data/taxRates";

            var results = await GetPaginatedResultsAsync<TaxRateContainer, TaxRate>(companyId, kind, baseUrl, ct);
            return results;
        }
        
        public async Task<CodatPayload> GetItemsAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "Items";
            var baseUrl = $"/companies/{companyId}/data/items";

            var results = await GetPaginatedResultsAsync<ItemContainer, Item>(companyId, kind, baseUrl, ct);
            return results;
        }

        public async Task<Guid> GetConnectionIdAsync(Guid companyId, CancellationToken ct)
        {
            try
            {
                await _companyIdsToConnectionIdsLock.WaitAsync(ct);
                if (_companyIdsToConnectionIds.TryGetValue(companyId, out var connectionId))
                {
                    return connectionId;
                }
                
                var url = $"/companies/{companyId}";
                using (var resp = await _codatClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct))
                {
                    resp.EnsureSuccessStatusCode();
                    var json = await resp.Content.ReadAsStringAsync(ct);
                    var codatIntegration = JsonConvert.DeserializeObject<AccountingIntegration>(json, _jsonSettings);
                    var accountingConnection = codatIntegration.DataConnections
                        .Single(c => string.Equals(c.SourceType, "Accounting", StringComparison.OrdinalIgnoreCase));

                    var guid = Guid.Parse(accountingConnection.Id);
                    _companyIdsToConnectionIds[companyId] = guid;
                    return guid;
                }
            }
            finally
            {
                _companyIdsToConnectionIdsLock.Release();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="baseUrlSlug">Do NOT append page number and page size querystring elements</param>
        /// <param name="ct"></param>
        /// <param name="companyId"></param>
        /// <typeparam name="T">The container type, i.e. the type that contains a get-only List<U> Results property</typeparam>
        /// <typeparam name="U">The type associated with the Results field within the container</typeparam>
        /// <returns></returns>
        private async Task<CodatPayload> GetPaginatedResultsAsync<T, U>(Guid companyId, string kind, string baseUrlSlug, CancellationToken ct)
            where T : IPaginated<U>
        {
            const int maxPageSize = 5_000;
            // const int maxPageSize = 5;
            const int firstPage = 1;

            if (string.IsNullOrWhiteSpace(baseUrlSlug))
                throw new ArgumentNullException(nameof(baseUrlSlug));
            if (baseUrlSlug.Contains("pageSize", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"{nameof(baseUrlSlug)} should not have pageSize specified");
            if (baseUrlSlug.Contains("page=", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"{nameof(baseUrlSlug)} should not have page number specified");
            
            var separator = baseUrlSlug.Contains("?") ? "&" : "?";
            var next = $"{baseUrlSlug}{separator}pageSize={maxPageSize}&page=1";

            var containers = new List<T>();
            var pages = 0;
            var timer = Stopwatch.StartNew();
            while (!string.IsNullOrEmpty(next))
            {
                pages++;
                using (var resp = await _codatClient.GetAsync(next, HttpCompletionOption.ResponseHeadersRead, ct))
                {
                    resp.EnsureSuccessStatusCode();
                    var json = await resp.Content.ReadAsStringAsync(ct);
                    var deserialized = JsonConvert.DeserializeObject<T>(json, _jsonSettings);
                    containers.Add(deserialized);
                    next = deserialized.Links?.Next?.Link;
                }
            }
            timer.Stop();

            var collector = containers.First();
            if (containers.Count > 1)
            {
                var results = containers
                    .SelectMany(b => b.Results)
                    .ToList();
            
                collector.Results.Clear();
                collector.Results.AddRange(results);
            }

            return new CodatPayload
            {
                CodatId = companyId,
                Kind = kind,
                Json = JsonConvert.SerializeObject(collector, _jsonSettings),
                PageCount = pages,
                Duration = timer.Elapsed,
            };
        }

        private async Task<CodatPayload> DownloadPayloadAsync(Guid companyId, string url, string kind, CancellationToken ct)
        {
            var timer = Stopwatch.StartNew();
            using (var resp = await _codatClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct))
            {
                resp.EnsureSuccessStatusCode();
                var json = await resp.Content.ReadAsStringAsync(ct);
                timer.Stop();
                return new CodatPayload
                {
                    CodatId = companyId,
                    Kind = kind,
                    Json = json,
                    PageCount = 1,
                    Duration = timer.Elapsed,
                };
            }
        }
    }
}