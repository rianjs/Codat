using System;
using System.Threading;
using System.Threading.Tasks;

namespace Connector
{
    public interface IAccountingDataReader
    {
        Task<CodatPayload> GetBalanceSheetAsync(Guid companyId, CancellationToken ct);
        Task<CodatPayload> GetBankAccounts(Guid companyId, CancellationToken ct);
        Task<CodatPayload> GetBillsAsync(Guid companyId, CancellationToken ct);
        Task<CodatPayload> GetChartOfAccountsAsync(Guid companyId, CancellationToken ct);
        Task<CodatPayload> GetCompanyInfoAsync(Guid companyId, CancellationToken ct);
        Task<CodatPayload> GetCreditNotesAsync(Guid companyId, CancellationToken ct);
        Task<CodatPayload> GetCustomersAsync(Guid companyId, CancellationToken ct);
        Task<CodatPayload> GetBankTransactionsAsync(Guid companyId, CancellationToken ct);
        Task<CodatPayload> GetInvoicesAsync(Guid companyId, CancellationToken ct);
        Task<CodatPayload> GetPaymentsAsync(Guid companyId, CancellationToken ct);
        Task<CodatPayload> GetProfitAndLossAsync(Guid companyId, CancellationToken ct);
        Task<CodatPayload> GetSuppliersAsync(Guid companyId, CancellationToken ct);
        Task<CodatPayload> GetJournalEntriesAsync(Guid companyId, CancellationToken ct);
        Task<CodatPayload> GetBillPaymentsAsync(Guid companyId, CancellationToken ct);
        Task<CodatPayload> GetTaxRatesAsync(Guid companyId, CancellationToken ct);
        Task<CodatPayload> GetItemsAsync(Guid companyId, CancellationToken ct);
        Task<Guid> GetConnectionIdAsync(Guid companyId, CancellationToken ct);
    }
}