using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Ap.Web
{
    public class InvokingApiLoggingMemoryStore : IInvokingApiLoggingStore
    {
        private readonly static int MaxLimits=101;
        ConcurrentDictionary<string,InvokingApiLoggingEntry> cache=new ConcurrentDictionary<string,InvokingApiLoggingEntry>();
        ConcurrentQueue<string> sweepQueue=new ConcurrentQueue<string>();
        public Task PutAsync(InvokingApiLoggingEntry entry)
        {
            if(cache.TryAdd(entry.TraceId,entry))
            {
                sweepQueue.Enqueue(entry.TraceId);

                if (sweepQueue.Count>=MaxLimits && sweepQueue.TryDequeue(out string traceId))
                {
                    cache.TryRemove(traceId,out InvokingApiLoggingEntry removedEntry);
                }
            }

            return Task.CompletedTask;
        }

        public ValueTask<InvokingApiLoggingEntry> GetAsync(string traceId)
        {
            if(cache.TryGetValue(traceId,out InvokingApiLoggingEntry entry))
            {
                return new ValueTask<InvokingApiLoggingEntry>(entry);
            }
            return new ValueTask<InvokingApiLoggingEntry>((InvokingApiLoggingEntry)null);
        }
    }
}