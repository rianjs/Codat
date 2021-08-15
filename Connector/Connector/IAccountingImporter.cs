using System;
using System.Threading;
using System.Threading.Tasks;

namespace Connector
{
    public interface IAccountingImporter
    {
        Task ImportDataAsync(Guid companyId, CancellationToken ct);
    }
}