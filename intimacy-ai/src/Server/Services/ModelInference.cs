using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace IntimacyAI.Server.Services
{
    public sealed class OnnxOptions
    {
        public bool Enabled { get; set; }
        public string? ModelPath { get; set; }
        public string Provider { get; set; } = "cpu"; // cpu, cuda
        public int IntraOpNumThreads { get; set; } = 1;
        public int InterOpNumThreads { get; set; } = 1;
    }

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

    // ONNX Runtime implementation
    public sealed class OnnxModelInferenceService : IModelInferenceService, IDisposable
    {
        private readonly Microsoft.ML.OnnxRuntime.InferenceSession _session;
        private readonly OnnxOptions _options;

        public OnnxModelInferenceService(IOptions<OnnxOptions> options)
        {
            _options = options.Value;
            if (string.IsNullOrWhiteSpace(_options.ModelPath))
                throw new InvalidOperationException("Onnx:ModelPath is required when ONNX is enabled.");

            var sessOptions = new Microsoft.ML.OnnxRuntime.SessionOptions
            {
                IntraOpNumThreads = _options.IntraOpNumThreads,
                InterOpNumThreads = _options.InterOpNumThreads,
            };

            if (string.Equals(_options.Provider, "cuda", StringComparison.OrdinalIgnoreCase))
            {
                try { sessOptions.AppendExecutionProvider_CUDA(); }
                catch { /* fallback to CPU if CUDA provider unavailable */ }
            }

            _session = new Microsoft.ML.OnnxRuntime.InferenceSession(_options.ModelPath, sessOptions);
        }

        public Task<ModelInferenceResult> AnalyzeImageAsync(byte[] imageData, IDictionary<string, string>? metadata, CancellationToken cancellationToken = default)
        {
            // Basic image preprocessing to float tensor NCHW (1,3,224,224) normalized 0..1
            using var image = Image.Load<Rgba32>(imageData);
            const int targetWidth = 224;
            const int targetHeight = 224;
            image.Mutate(ctx => ctx.Resize(targetWidth, targetHeight));

            var tensor = new Microsoft.ML.OnnxRuntime.Tensors.DenseTensor<float>(new[] { 1, 3, targetHeight, targetWidth });
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < targetHeight; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < targetWidth; x++)
                    {
                        var p = row[x];
                        tensor[0, 0, y, x] = p.R / 255f;
                        tensor[0, 1, y, x] = p.G / 255f;
                        tensor[0, 2, y, x] = p.B / 255f;
                    }
                }
            });

            // Find first input and output names
            var inputName = _session.InputMetadata.Keys.First();
            var outputName = _session.OutputMetadata.Keys.First();

            var inputs = new List<Microsoft.ML.OnnxRuntime.NamedOnnxValue>
            {
                Microsoft.ML.OnnxRuntime.NamedOnnxValue.CreateFromTensor(inputName, tensor)
            };

            using var results = _session.Run(inputs);
            var first = results.First(r => r.Name == outputName).AsEnumerable<float>().ToArray();

            // Map scores to generic keys
            var scores = new Dictionary<string, object>();
            for (int i = 0; i < first.Length; i++)
            {
                scores[$"score_{i}"] = first[i];
            }

            var md = new Dictionary<string, string>(metadata ?? new Dictionary<string, string>())
            {
                ["width"] = targetWidth.ToString(),
                ["height"] = targetHeight.ToString(),
                ["provider"] = _options.Provider,
                ["modelPath"] = _options.ModelPath ?? string.Empty
            };

            var result = new ModelInferenceResult
            {
                Scores = scores,
                Metadata = md
            };

            return Task.FromResult(result);
        }

        public void Dispose()
        {
            _session.Dispose();
        }
    }
}

