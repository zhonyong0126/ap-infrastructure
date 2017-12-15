using System.Threading.Tasks;

namespace Ap.Web
{
    public interface IInvokingApiLoggingStore
    {
        Task PutAsync(InvokingApiLoggingEntry entry);
        ValueTask<InvokingApiLoggingEntry> GetAsync( string traceId);
    }
}