using System;
using System.Threading;
using System.Threading.Tasks;

namespace Connector
{
    public class AccountingWorker
    {
        private readonly IAccountingDataReader _reader;

        public AccountingWorker(IAccountingDataReader reader)
        {
            _reader = reader;
        }

        public async Task ImportDataAsync(Guid companyId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}