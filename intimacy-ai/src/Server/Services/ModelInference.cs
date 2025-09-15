using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace IntimacyAI.Server.Services
{
    public sealed class ModelInferenceResult
    {
        public Dictionary<string, object> Scores { get; init; } = new();
        public Dictionary<string, string> Metadata { get; init; } = new();
    }

    public interface IModelInferenceService
    {
        Task<ModelInferenceResult> AnalyzeImageAsync(byte[] imageData, IDictionary<string, string>? metadata, CancellationToken cancellationToken = default);
    }

    // Placeholder heuristic implementation. Replace with a real model integration.
    public sealed class PlaceholderModelInferenceService : IModelInferenceService
    {
        public Task<ModelInferenceResult> AnalyzeImageAsync(byte[] imageData, IDictionary<string, string>? metadata, CancellationToken cancellationToken = default)
        {
            double arousalScore = 0.0;
            double engagementScore = 0.0;
            int width = 0, height = 0;

            try
            {
                using var image = Image.Load<Rgba32>(imageData);
                width = image.Width;
                height = image.Height;
                double sum = 0, sumSq = 0; long n = 0;
                image.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        var row = accessor.GetRowSpan(y);
                        for (int x = 0; x < row.Length; x++)
                        {
                            var p = row[x];
                            double l = 0.2126 * p.R + 0.7152 * p.G + 0.0722 * p.B;
                            sum += l;
                            sumSq += l * l;
                            n++;
                        }
                    }
                });
                if (n > 0)
                {
                    double mean = sum / n;
                    double var = Math.Max(0, (sumSq / n) - (mean * mean));
                    double std = Math.Sqrt(var);
                    arousalScore = Math.Clamp(mean / 255.0, 0.0, 1.0);
                    engagementScore = Math.Clamp(std / 128.0, 0.0, 1.0);
                }
            }
            catch
            {
                // keep defaults if decoding fails
            }

            var scoresObj = new Dictionary<string, object>
            {
                ["arousal"] = arousalScore,
                ["engagement"] = engagementScore
            };
            var md = new Dictionary<string, string>(metadata ?? new Dictionary<string, string>())
            {
                ["width"] = width.ToString(),
                ["height"] = height.ToString()
            };

            var result = new ModelInferenceResult
            {
                Scores = scoresObj,
                Metadata = md
            };

            return Task.FromResult(result);
        }
    }
}

