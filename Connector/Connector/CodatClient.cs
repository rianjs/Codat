using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Connector.CodatTypes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Connector
{
    public class CodatClient :
        IAccountingDataReader
    {
        private readonly int _lookbackMonths;
        private readonly HttpClient _codatClient;
        private readonly JsonSerializerSettings _jsonSettings;
        private readonly Dictionary<Guid, Guid> _companyIdsToConnectionIds;
        private readonly SemaphoreSlim _companyIdsToConnectionIdsLock = new(1, 1);
        private readonly ILogger<IAccountingDataReader> _log;

        public CodatClient(int lookbackMonths, HttpClient codatClient, JsonSerializerSettings jsonSettings, ILogger<IAccountingDataReader> log)
        {
            _lookbackMonths = lookbackMonths < 1
                ? throw new ArgumentOutOfRangeException($"{nameof(lookbackMonths)} should be a positive integer, usually 24-26 months")
                : lookbackMonths;
            _codatClient = codatClient ?? throw new ArgumentNullException(nameof(codatClient));
            _jsonSettings = jsonSettings ?? throw new ArgumentNullException(nameof(jsonSettings));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            
            _companyIdsToConnectionIds = new Dictionary<Guid, Guid>();
        }

        public async Task<CodatPayload> GetBalanceSheetAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "BalanceSheet";
            const int periodLength = 30;
            var baseUrl = $"/companies/{companyId}/data/financials/balanceSheet?periodLength={periodLength}&periodsToCompare={_lookbackMonths}";
            
            _log.LogInformation($"{kind} ingestion for company id {companyId} beginning with url ' {baseUrl} '");
            
            var timer = Stopwatch.StartNew();
            var result = await DownloadPayloadAsync(companyId, baseUrl, kind, ct);
            timer.Stop();
            
            _log.LogInformation($"{kind} ingestion completed for company id {companyId} completed in {timer.ElapsedMilliseconds:N0}ms");
            return result;
        }

        public async Task<CodatPayload> GetBankAccounts(Guid companyId, CancellationToken ct)
        {
            const string kind = "BankAccounts";
            var connectionId = await GetConnectionIdAsync(companyId, ct);

            var baseUrl = $"/companies/{companyId}/connections/{connectionId}/data/bankAccounts";
            
            _log.LogInformation($"{kind} ingestion for company id {companyId} beginning with url ' {baseUrl} '");
            
            var timer = Stopwatch.StartNew();
            var result = await GetPaginatedResultsAsync<BankAccountContainer, BankAccount>(companyId, kind, baseUrl, ct);
            timer.Stop();
            
            _log.LogInformation($"{kind} ingestion completed for company id {companyId} completed in {timer.ElapsedMilliseconds:N0}ms");
            return result;
        }

        public async Task<CodatPayload> GetBillsAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "Bills";
            var baseUrl = $"/companies/{companyId}/data/bills";
            
            _log.LogInformation($"{kind} ingestion for company id {companyId} beginning with url ' {baseUrl} '");
            
            var timer = Stopwatch.StartNew();
            var result = await GetPaginatedResultsAsync<BillContainer, Bill>(companyId, kind, baseUrl, ct);
            timer.Stop();
            
            _log.LogInformation($"{kind} ingestion completed for company id {companyId} completed in {timer.ElapsedMilliseconds:N0}ms");
            return result;
        }

        public async Task<CodatPayload> GetChartOfAccountsAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "ChartOfAccounts";
            var baseUrl = $"/companies/{companyId}/data/accounts";
            
            _log.LogInformation($"{kind} ingestion for company id {companyId} beginning with url ' {baseUrl} '");
            
            var timer = Stopwatch.StartNew();
            var result = await GetPaginatedResultsAsync<AccountsContainer, Account>(companyId, kind, baseUrl, ct);
            timer.Stop();
            
            _log.LogInformation($"{kind} ingestion completed for company id {companyId} completed in {timer.ElapsedMilliseconds:N0}ms");
            return result;
        }

        public async Task<CodatPayload> GetCompanyInfoAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "CompanyInfo";
            var baseUrl = $"/companies/{companyId}/data/info";
            
            _log.LogInformation($"{kind} ingestion for company id {companyId} beginning with url ' {baseUrl} '");
            
            var timer = Stopwatch.StartNew();
            var result = await DownloadPayloadAsync(companyId, baseUrl, kind, ct);
            timer.Stop();
            
            _log.LogInformation($"{kind} ingestion completed for company id {companyId} completed in {timer.ElapsedMilliseconds:N0}ms");
            return result;
        }

        public async Task<CodatPayload> GetCreditNotesAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "CreditNotes";
            var baseUrl = $"/companies/{companyId}/data/creditNotes";
            
            _log.LogInformation($"{kind} ingestion for company id {companyId} beginning with url ' {baseUrl} '");
            
            var timer = Stopwatch.StartNew();
            var result = await GetPaginatedResultsAsync<CreditNotesContainer, CreditNotes>(companyId, kind, baseUrl, ct);
            timer.Stop();
            
            _log.LogInformation($"{kind} ingestion completed for company id {companyId} completed in {timer.ElapsedMilliseconds:N0}ms");
            return result;
        }

        public async Task<CodatPayload> GetCustomersAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "Customers";
            var baseUrl = $"/companies/{companyId}/data/customers";
            
            _log.LogInformation($"{kind} ingestion for company id {companyId} beginning with url ' {baseUrl} '");
            
            var timer = Stopwatch.StartNew();
            var result = await GetPaginatedResultsAsync<CustomersContainer, Customer>(companyId, kind, baseUrl, ct);
            timer.Stop();
            
            _log.LogInformation($"{kind} ingestion completed for company id {companyId} completed in {timer.ElapsedMilliseconds:N0}ms");
            return result;
        }
        
        public async Task<CodatPayload> GetBankTransactionsAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "BankTransactions";
            
            _log.LogInformation($"Fetched bank account information for company id {companyId} so we can fetch transactions");
            var bankAcctPayloads = await GetBankAccounts(companyId, ct);
            _log.LogInformation($"Bank account information for company id {companyId} fetched, beginning transaction ingestion");
            
            var deserializedBankAccounts = CompressionUtils.FromJsonSerializedGzipBytes<BankAccountContainer>(bankAcctPayloads.GzipJson, _jsonSettings);
            var transactionsQueries = deserializedBankAccounts.Results
                .Select(a => a.Id)
                .Select(accountId =>
                {
                    var baseUrl = $"/companies/{companyId}/data/bankAccounts/{accountId}/transactions";
                    return new KeyValuePair<string, Task<CodatPayload>>(accountId, GetPaginatedResultsAsync<BankTransactionContainer, BankTransaction>(companyId, kind, baseUrl, ct));
                })
                .ToDictionary(q => q.Key, q => q.Value);
            
            try
            {
                await Task.WhenAll(transactionsQueries.Values);
            }
            catch (Exception e)
            {
                // bank transactions don't work for everything, and this is normal...
            }

            var unsupportedAccounts = transactionsQueries.Where(q => !q.Value.IsCompletedSuccessfully);
            foreach (var transaction in unsupportedAccounts)
            {
                _log.LogInformation($"Bank account {transaction.Key} for company id {companyId} does not support fetching transaction");
            }

            var transactionsByAccountId = transactionsQueries
                .Where(q => q.Value.IsCompletedSuccessfully)
                // BankTransactionContainers do not have account ids as part of their data structure
                .Select(r => new KeyValuePair<string, BankTransactionContainer>(
                    r.Key,
                    CompressionUtils.FromJsonSerializedGzipBytes<BankTransactionContainer>(r.Value.Result.GzipJson, _jsonSettings)))
                .ToDictionary(r => r.Key, r => r.Value);
            
            var serialized = CompressionUtils.ToJsonSerializedGzipBytes(transactionsByAccountId, _jsonSettings);
            return new CodatPayload
            {
                CodatId = companyId,
                Kind = kind,
                GzipJson = serialized,
            };
        }

        public async Task<CodatPayload> GetInvoicesAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "Invoices";
            var baseUrl = $"/companies/{companyId}/data/invoices";

            _log.LogInformation($"{kind} ingestion for company id {companyId} beginning with url ' {baseUrl} '");
            
            var timer = Stopwatch.StartNew();
            var result = await GetPaginatedResultsAsync<InvoicesContainer, Invoice>(companyId, kind, baseUrl, ct);
            timer.Stop();
            
            _log.LogInformation($"{kind} ingestion completed for company id {companyId} completed in {timer.ElapsedMilliseconds:N0}ms");
            return result;
        }

        public async Task<CodatPayload> GetPaymentsAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "Payments";
            var baseUrl = $"/companies/{companyId}/data/payments";
            
            _log.LogInformation($"{kind} ingestion for company id {companyId} beginning with url ' {baseUrl} '");
            
            var timer = Stopwatch.StartNew();
            var result = await GetPaginatedResultsAsync<PaymentsContainer, Payment>(companyId, kind, baseUrl, ct);
            timer.Stop();
            
            _log.LogInformation($"{kind} ingestion completed for company id {companyId} completed in {timer.ElapsedMilliseconds:N0}ms");
            return result;
        }

        public async Task<CodatPayload> GetProfitAndLossAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "ProfitAndLoss";
            var baseUrl = $"/companies/{companyId}/data/financials/profitAndLoss?periodLength=30&periodsToCompare={_lookbackMonths}";
            
            _log.LogInformation($"{kind} ingestion for company id {companyId} beginning with url ' {baseUrl} '");
            
            var timer = Stopwatch.StartNew();
            var result = await DownloadPayloadAsync(companyId, baseUrl, kind, ct);
            timer.Stop();
            
            _log.LogInformation($"{kind} ingestion completed for company id {companyId} completed in {timer.ElapsedMilliseconds:N0}ms");
            return result;
        }

        public async Task<CodatPayload> GetSuppliersAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "Suppliers";
            var baseUrl = $"/companies/{companyId}/data/suppliers";
            
            _log.LogInformation($"{kind} ingestion for company id {companyId} beginning with url ' {baseUrl} '");
            
            var timer = Stopwatch.StartNew();
            var result = await GetPaginatedResultsAsync<SupplierContainer, Supplier>(companyId, kind, baseUrl, ct);
            timer.Stop();
            
            _log.LogInformation($"{kind} ingestion completed for company id {companyId} completed in {timer.ElapsedMilliseconds:N0}ms");
            return result;
        }
        
        public async Task<CodatPayload> GetJournalEntriesAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "JournalEntries";
            var baseUrl = $"/companies/{companyId}/data/journalEntries";
            
            _log.LogInformation($"{kind} ingestion for company id {companyId} beginning with url ' {baseUrl} '");
            
            var timer = Stopwatch.StartNew();
            var result = await GetPaginatedResultsAsync<JournalEntryContainer, JournalEntry>(companyId, kind, baseUrl, ct);
            timer.Stop();
            
            _log.LogInformation($"{kind} ingestion completed for company id {companyId} completed in {timer.ElapsedMilliseconds:N0}ms");
            return result;
        }

        public async Task<CodatPayload> GetBillPaymentsAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "BillPayments";
            var baseUrl = $"/companies/{companyId}/data/billPayments";
            
            _log.LogInformation($"{kind} ingestion for company id {companyId} beginning with url ' {baseUrl} '");
            
            var timer = Stopwatch.StartNew();
            var result = await GetPaginatedResultsAsync<BillPaymentContainer, BillPayment>(companyId, kind, baseUrl, ct);
            timer.Stop();
            
            _log.LogInformation($"{kind} ingestion completed for company id {companyId} completed in {timer.ElapsedMilliseconds:N0}ms");
            return result;
        }

        public async Task<CodatPayload> GetTaxRatesAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "TaxRates";
            var baseUrl = $"/companies/{companyId}/data/taxRates";
            
            _log.LogInformation($"{kind} ingestion for company id {companyId} beginning with url ' {baseUrl} '");
            
            var timer = Stopwatch.StartNew();
            var result = await GetPaginatedResultsAsync<TaxRateContainer, TaxRate>(companyId, kind, baseUrl, ct);
            timer.Stop();
            
            _log.LogInformation($"{kind} ingestion completed for company id {companyId} completed in {timer.ElapsedMilliseconds:N0}ms");
            return result;
        }
        
        public async Task<CodatPayload> GetItemsAsync(Guid companyId, CancellationToken ct)
        {
            const string kind = "Items";
            var baseUrl = $"/companies/{companyId}/data/items";
            
            _log.LogInformation($"{kind} ingestion for company id {companyId} beginning with url ' {baseUrl} '");
            
            var timer = Stopwatch.StartNew();
            var result = await GetPaginatedResultsAsync<ItemContainer, Item>(companyId, kind, baseUrl, ct);
            timer.Stop();
            
            _log.LogInformation($"{kind} ingestion completed for company id {companyId} completed in {timer.ElapsedMilliseconds:N0}ms");
            return result;
        }

        public async Task<Guid> GetConnectionIdAsync(Guid companyId, CancellationToken ct)
        {
            _log.LogInformation($"Getting the connection id for {companyId}");

            var timer = Stopwatch.StartNew();
            try
            {
                await _companyIdsToConnectionIdsLock.WaitAsync(ct);
                timer.Stop();
                _log.LogInformation($"Semaphore cache achieved in {timer.ElapsedMilliseconds:N0}ms for company id {companyId}");
                if (_companyIdsToConnectionIds.TryGetValue(companyId, out var connectionId))
                {
                    _log.LogInformation($"Fetched the connection id for {companyId} from cache in {timer.ElapsedMilliseconds:N0}ms");
                    return connectionId;
                }
                
                var url = $"/companies/{companyId}";
                _log.LogInformation($"Connection id for company id {companyId} is not in the cache, fetching from origin with url ' {url} '");
                
                timer = Stopwatch.StartNew();
                using (var resp = await _codatClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct))
                {
                    resp.EnsureSuccessStatusCode();
                    var json = await resp.Content.ReadAsStringAsync(ct);
                    var codatIntegration = JsonConvert.DeserializeObject<AccountingIntegration>(json, _jsonSettings);
                    var accountingConnection = codatIntegration.DataConnections
                        .Single(c => string.Equals(c.SourceType, "Accounting", StringComparison.OrdinalIgnoreCase));

                    var guid = Guid.Parse(accountingConnection.Id);
                    _companyIdsToConnectionIds[companyId] = guid;
                    
                    timer.Stop();
                    _log.LogInformation($"Fetched the connection id for company id {companyId} from origin and updated cache in {timer.ElapsedMilliseconds:N0}ms");
                    
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

            if (string.IsNullOrWhiteSpace(baseUrlSlug))
                throw new ArgumentNullException(nameof(baseUrlSlug));
            if (baseUrlSlug.Contains("pageSize", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"{nameof(baseUrlSlug)} should not have pageSize specified");
            if (baseUrlSlug.Contains("page=", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"{nameof(baseUrlSlug)} should not have page number specified");
            
            _log.LogInformation($"Ingesting paginated {kind} API for company id {companyId} with paginated url ' {baseUrlSlug} '");
            
            var separator = baseUrlSlug.Contains("?") ? "&" : "?";
            var urlSlug = $"{baseUrlSlug}{separator}pageSize={maxPageSize}&page=1";
            var currentUrl = urlSlug;
            
            var containers = new List<T>();
            var pages = 0;
            var payloadTimer = Stopwatch.StartNew();
            while (!string.IsNullOrEmpty(urlSlug))
            {
                pages++;
                _log.LogInformation($"Ingested page {pages} of {kind} for company id {companyId} with url {urlSlug}");
                
                var pageTimer = Stopwatch.StartNew();
                using (var resp = await _codatClient.GetAsync(urlSlug, HttpCompletionOption.ResponseHeadersRead, ct))
                {
                    resp.EnsureSuccessStatusCode();
                    var json = await resp.Content.ReadAsStringAsync(ct);
                    var deserialized = JsonConvert.DeserializeObject<T>(json, _jsonSettings);
                    containers.Add(deserialized);
                    currentUrl = urlSlug;
                    urlSlug = deserialized.Links?.Next?.Link;
                }
                pageTimer.Stop();
                _log.LogInformation($"Ingested page {pages} of {kind} for company id {companyId} with url {currentUrl} in {pageTimer.ElapsedMilliseconds:N0}ms");
            }

            var collector = containers.First();
            if (containers.Count > 1)
            {
                var results = containers
                    .SelectMany(b => b.Results)
                    .ToList();
            
                collector.Results.Clear();
                collector.Results.AddRange(results);
            }

            var result = new CodatPayload
            {
                CodatId = companyId,
                Kind = kind,
                GzipJson = CompressionUtils.ToJsonSerializedGzipBytes(collector, _jsonSettings),
                PageCount = pages,
                Duration = payloadTimer.Elapsed,
            };
            payloadTimer.Stop();
            
            _log.LogInformation($"Ingested paginated {kind} API for company id {companyId} with paginated url ' {baseUrlSlug} ' in {payloadTimer.ElapsedMilliseconds:N0}ms which had {pages:N0} pages");
            return result;
        }

        private async Task<CodatPayload> DownloadPayloadAsync(Guid companyId, string url, string kind, CancellationToken ct)
        {
            _log.LogInformation($"Ingesting {kind} API for company id {companyId} with url ' {url} '");
            
            var timer = Stopwatch.StartNew();
            using (var resp = await _codatClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct))
            {
                resp.EnsureSuccessStatusCode();
                var json = await resp.Content.ReadAsStringAsync(ct);
                timer.Stop();
                
                _log.LogInformation($"Ingested {kind} API for company id {companyId} with url ' {url} ' in {timer.ElapsedMilliseconds:N0}ms");
                return new CodatPayload
                {
                    CodatId = companyId,
                    Kind = kind,
                    GzipJson = CompressionUtils.CompressString(json),
                    PageCount = 1,
                    Duration = timer.Elapsed,
                };
            }
        }
    }
}