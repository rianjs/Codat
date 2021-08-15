using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Connector
{
    public class CodatS3AccountingImporter :
        IAccountingImporter
    {
        private readonly IAccountingDataReader _reader;
        private readonly IAccountingWriter _writer;
        private readonly ILogger<IAccountingImporter> _log;

        public CodatS3AccountingImporter(IAccountingDataReader reader, IAccountingWriter writer, ILogger<IAccountingImporter> logger)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ImportDataAsync(Guid companyId, CancellationToken ct)
        {
            _log.LogInformation($"Importing accounting data for company id {companyId}");
            var timer = Stopwatch.StartNew();
            var payloadTasks = new List<Task>
            {
                ImportBalanceSheetAsync(companyId, ct),
                ImportBankAccountsAsync(companyId, ct),
                ImportBillsAsync(companyId, ct),
                ImportChartOfAccountsAsync(companyId, ct),
                ImportCompanyInfoAsync(companyId, ct),
                ImportCreditNotesAsync(companyId, ct),
                ImportCustomersAsync(companyId, ct),
                ImportBankTransactionsAsync(companyId, ct),
                ImportInvoicesAsync(companyId, ct),
                ImportItemsAsync(companyId, ct),
                ImportPaymentsAsync(companyId, ct),
                ImportProfitAndLossAsync(companyId, ct),
                ImportJournalEntriesAsync(companyId, ct),
                ImportSuppliersAsync(companyId, ct),
                ImportBillPaymentsAsync(companyId, ct),
                ImportTaxRatesAsync(companyId, ct),
            };

            await Task.WhenAll(payloadTasks);
            timer.Stop();
            _log.LogInformation($"All payloads for company id {companyId} ingested in {timer.ElapsedMilliseconds:N0}ms");

            var failedDownloadCount = payloadTasks.Count(t => !t.IsCompletedSuccessfully);
            if (failedDownloadCount > 0)
            {
                _log.LogError($"{failedDownloadCount} payloads were not imported successfully");
            }
        }

        private async Task ImportBalanceSheetAsync(Guid companyId, CancellationToken ct)
        {
            _log.LogInformation($"{nameof(ImportBalanceSheetAsync)} beginning for company id {companyId}");

            var timer = Stopwatch.StartNew();
            var data = await _reader.GetBalanceSheetAsync(companyId, ct);
            var success = await _writer.SavePayloadAsync(data, ct);
            timer.Stop();
            
            if (!success)
            {
                _log.LogError($"{nameof(ImportBalanceSheetAsync)} did not complete successfully for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
            else
            {
                _log.LogInformation($"{nameof(ImportBalanceSheetAsync)} completed for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
        }
        
        private async Task ImportBankAccountsAsync(Guid companyId, CancellationToken ct)
        {
            _log.LogInformation($"{nameof(ImportBankAccountsAsync)} beginning for company id {companyId}");

            var timer = Stopwatch.StartNew();
            var data = await _reader.GetBankAccounts(companyId, ct);
            var success = await _writer.SavePayloadAsync(data, ct);
            timer.Stop();
            
            if (!success)
            {
                _log.LogError($"{nameof(ImportBankAccountsAsync)} did not complete successfully for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
            else
            {
                _log.LogInformation($"{nameof(ImportBankAccountsAsync)} completed for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
        }
        
        private async Task ImportBillsAsync(Guid companyId, CancellationToken ct)
        {
            _log.LogInformation($"{nameof(ImportBillsAsync)} beginning for company id {companyId}");

            var timer = Stopwatch.StartNew();
            var data = await _reader.GetBillsAsync(companyId, ct);
            var success = await _writer.SavePayloadAsync(data, ct);
            timer.Stop();
            
            if (!success)
            {
                _log.LogError($"{nameof(ImportBillsAsync)} did not complete successfully for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
            else
            {
                _log.LogInformation($"{nameof(ImportBillsAsync)} completed for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
        }
        
        private async Task ImportChartOfAccountsAsync(Guid companyId, CancellationToken ct)
        {
            _log.LogInformation($"{nameof(ImportChartOfAccountsAsync)} beginning for company id {companyId}");

            var timer = Stopwatch.StartNew();
            var data = await _reader.GetChartOfAccountsAsync(companyId, ct);
            var success = await _writer.SavePayloadAsync(data, ct);
            timer.Stop();
            
            if (!success)
            {
                _log.LogError($"{nameof(ImportChartOfAccountsAsync)} did not complete successfully for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
            else
            {
                _log.LogInformation($"{nameof(ImportChartOfAccountsAsync)} completed for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
        }
        
        private async Task ImportCompanyInfoAsync(Guid companyId, CancellationToken ct)
        {
            _log.LogInformation($"{nameof(ImportCompanyInfoAsync)} beginning for company id {companyId}");

            var timer = Stopwatch.StartNew();
            var data = await _reader.GetCompanyInfoAsync(companyId, ct);
            var success = await _writer.SavePayloadAsync(data, ct);
            
            timer.Stop();
            if (!success)
            {
                _log.LogError($"{nameof(ImportCompanyInfoAsync)} did not complete successfully for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
            else
            {
                _log.LogInformation($"{nameof(ImportCompanyInfoAsync)} completed for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
        }
        
        private async Task ImportCreditNotesAsync(Guid companyId, CancellationToken ct)
        {
            _log.LogInformation($"{nameof(ImportCreditNotesAsync)} beginning for company id {companyId}");

            var timer = Stopwatch.StartNew();
            var data = await _reader.GetCreditNotesAsync(companyId, ct);
            var success = await _writer.SavePayloadAsync(data, ct);
            timer.Stop();
            
            if (!success)
            {
                _log.LogError($"{nameof(ImportCreditNotesAsync)} did not complete successfully for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
            else
            {
                _log.LogInformation($"{nameof(ImportCreditNotesAsync)} completed for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
        }
        
        private async Task ImportCustomersAsync(Guid companyId, CancellationToken ct)
        {
            _log.LogInformation($"{nameof(ImportCustomersAsync)} beginning for company id {companyId}");

            var timer = Stopwatch.StartNew();
            var data = await _reader.GetCustomersAsync(companyId, ct);
            var success = await _writer.SavePayloadAsync(data, ct);
            timer.Stop();
            
            if (!success)
            {
                _log.LogError($"{nameof(ImportCustomersAsync)} did not complete successfully for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
            else
            {
                _log.LogInformation($"{nameof(ImportCustomersAsync)} completed for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
        }
        
        private async Task ImportBankTransactionsAsync(Guid companyId, CancellationToken ct)
        {
            _log.LogInformation($"{nameof(ImportBankTransactionsAsync)} beginning for company id {companyId}");

            var timer = Stopwatch.StartNew();
            var data = await _reader.GetBankTransactionsAsync(companyId, ct);
            var success = await _writer.SavePayloadAsync(data, ct);
            timer.Stop();
            
            if (!success)
            {
                _log.LogError($"{nameof(ImportBankTransactionsAsync)} did not complete successfully for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
            else
            {
                _log.LogInformation($"{nameof(ImportBankTransactionsAsync)} completed for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
        }
        
        private async Task ImportInvoicesAsync(Guid companyId, CancellationToken ct)
        {
            _log.LogInformation($"{nameof(ImportInvoicesAsync)} beginning for company id {companyId}");

            var timer = Stopwatch.StartNew();
            var data = await _reader.GetInvoicesAsync(companyId, ct);
            var success = await _writer.SavePayloadAsync(data, ct);
            timer.Stop();
            
            if (!success)
            {
                _log.LogError($"{nameof(ImportInvoicesAsync)} did not complete successfully for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
            else
            {
                _log.LogInformation($"{nameof(ImportInvoicesAsync)} completed for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
        }
        
        private async Task ImportItemsAsync(Guid companyId, CancellationToken ct)
        {
            _log.LogInformation($"{nameof(ImportItemsAsync)} beginning for company id {companyId}");

            var timer = Stopwatch.StartNew();
            var data = await _reader.GetItemsAsync(companyId, ct);
            var success = await _writer.SavePayloadAsync(data, ct);
            timer.Stop();
            
            if (!success)
            {
                _log.LogError($"{nameof(ImportItemsAsync)} did not complete successfully for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
            else
            {
                _log.LogInformation($"{nameof(ImportItemsAsync)} completed for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
        }
        
        private async Task ImportPaymentsAsync(Guid companyId, CancellationToken ct)
        {
            _log.LogInformation($"{nameof(ImportPaymentsAsync)} beginning for company id {companyId}");

            var timer = Stopwatch.StartNew();
            var data = await _reader.GetPaymentsAsync(companyId, ct);
            var success = await _writer.SavePayloadAsync(data, ct);
            
            timer.Stop();
            if (!success)
            {
                _log.LogError($"{nameof(ImportPaymentsAsync)} did not complete successfully for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
            else
            {
                _log.LogInformation($"{nameof(ImportPaymentsAsync)} completed for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
        }
        
        private async Task ImportProfitAndLossAsync(Guid companyId, CancellationToken ct)
        {
            _log.LogInformation($"{nameof(ImportProfitAndLossAsync)} beginning for company id {companyId}");

            var timer = Stopwatch.StartNew();
            var data = await _reader.GetProfitAndLossAsync(companyId, ct);
            var success = await _writer.SavePayloadAsync(data, ct);
            timer.Stop();
            
            if (!success)
            {
                _log.LogError($"{nameof(ImportProfitAndLossAsync)} did not complete successfully for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
            else
            {
                _log.LogInformation($"{nameof(ImportProfitAndLossAsync)} completed for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
        }
        
        private async Task ImportJournalEntriesAsync(Guid companyId, CancellationToken ct)
        {
            _log.LogInformation($"{nameof(ImportJournalEntriesAsync)} beginning for company id {companyId}");

            var timer = Stopwatch.StartNew();
            var data = await _reader.GetJournalEntriesAsync(companyId, ct);
            var success = await _writer.SavePayloadAsync(data, ct);
            timer.Stop();
            
            if (!success)
            {
                _log.LogError($"{nameof(ImportJournalEntriesAsync)} did not complete successfully for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
            else
            {
                _log.LogInformation($"{nameof(ImportJournalEntriesAsync)} completed for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
        }
        
        private async Task ImportSuppliersAsync(Guid companyId, CancellationToken ct)
        {
            _log.LogInformation($"{nameof(ImportSuppliersAsync)} beginning for company id {companyId}");

            var timer = Stopwatch.StartNew();
            var data = await _reader.GetSuppliersAsync(companyId, ct);
            var success = await _writer.SavePayloadAsync(data, ct);
            timer.Stop();
            
            if (!success)
            {
                _log.LogError($"{nameof(ImportSuppliersAsync)} did not complete successfully for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
            else
            {
                _log.LogInformation($"{nameof(ImportSuppliersAsync)} completed for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
        }
        
        private async Task ImportBillPaymentsAsync(Guid companyId, CancellationToken ct)
        {
            _log.LogInformation($"{nameof(ImportBillPaymentsAsync)} beginning for company id {companyId}");

            var timer = Stopwatch.StartNew();
            var data = await _reader.GetBillPaymentsAsync(companyId, ct);
            var success = await _writer.SavePayloadAsync(data, ct);
            timer.Stop();
            
            if (!success)
            {
                _log.LogError($"{nameof(ImportBillPaymentsAsync)} did not complete successfully for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
            else
            {
                _log.LogInformation($"{nameof(ImportBillPaymentsAsync)} completed for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
        }
        
        private async Task ImportTaxRatesAsync(Guid companyId, CancellationToken ct)
        {
            _log.LogInformation($"{nameof(ImportTaxRatesAsync)} beginning for company id {companyId}");

            var timer = Stopwatch.StartNew();
            var data = await _reader.GetTaxRatesAsync(companyId, ct);
            var success = await _writer.SavePayloadAsync(data, ct);
            timer.Stop();
            
            if (!success)
            {
                _log.LogError($"{nameof(ImportTaxRatesAsync)} did not complete successfully for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
            else
            {
                _log.LogInformation($"{nameof(ImportTaxRatesAsync)} completed for company id {companyId} in {timer.ElapsedMilliseconds}");
            }
        }
    }
}