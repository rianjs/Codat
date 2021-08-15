using System.Threading;
using System.Threading.Tasks;

namespace Connector
{
    public interface IAccountingPersistor
    {
        Task SavePayloadAsync(CodatPayload payload, CancellationToken ct);
    }
}