using System.Threading;
using System.Threading.Tasks;

namespace Connector
{
    public interface IAccountingWriter
    {
        Task<bool> SavePayloadAsync(CodatPayload payload, CancellationToken ct);
    }
}