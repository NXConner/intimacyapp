using System.Collections.Concurrent;

namespace IntimacyAI.Server.Services
{
    public sealed class AnalysisRequest
    {
        public string SessionId { get; init; } = Guid.NewGuid().ToString("n");
        public string AnalysisType { get; init; } = "image";
        public byte[] Data { get; init; } = Array.Empty<byte>();
        public IDictionary<string, string>? Metadata { get; init; }
    }

    public interface IAnalysisQueue
    {
        void Enqueue(AnalysisRequest request);
        bool TryDequeue(out AnalysisRequest? request);
    }

    public sealed class AnalysisQueue : IAnalysisQueue
    {
        private readonly ConcurrentQueue<AnalysisRequest> _queue = new();

        public void Enqueue(AnalysisRequest request) => _queue.Enqueue(request);

        public bool TryDequeue(out AnalysisRequest? request)
        {
            var ok = _queue.TryDequeue(out var r);
            request = r;
            return ok;
        }
    }
}

