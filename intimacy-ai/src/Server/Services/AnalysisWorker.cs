using IntimacyAI.Server.Data;
using IntimacyAI.Server.Models;
using IntimacyAI.Server.Hubs;
using IntimacyAI.Server.Security;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace IntimacyAI.Server.Services
{
    public sealed class AnalysisWorker : BackgroundService
    {
        private readonly ILogger<AnalysisWorker> _logger;
        private readonly IServiceProvider _services;
        private readonly IAnalysisQueue _queue;
        private readonly IHubContext<AnalysisHub> _hubContext;

        public AnalysisWorker(ILogger<AnalysisWorker> logger, IServiceProvider services, IAnalysisQueue queue, IHubContext<AnalysisHub> hubContext)
        {
            _logger = logger;
            _services = services;
            _queue = queue;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_queue.TryDequeue(out var req) && req is not null)
                    {
                        // Notify job started
                        await _hubContext.Clients.All.SendAsync("analysisStarted", req.SessionId, cancellationToken: stoppingToken);

                        // Minimal luminance-based scoring from image bytes
                        double arousalScore = 0.0;
                        double engagementScore = 0.0;
                        int width = 0, height = 0;
                        try
                        {
                            using var image = Image.Load<Rgba32>(req.Data);
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
                        var scoresObj = new { arousal = arousalScore, engagement = engagementScore };
                        var metadata = req.Metadata ?? new Dictionary<string, string>();
                        metadata["width"] = width.ToString();
                        metadata["height"] = height.ToString();

                        using var scope = _services.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var enc = scope.ServiceProvider.GetRequiredService<IEncryptionService>();

                        var record = new AnalysisHistory
                        {
                            SessionId = req.SessionId,
                            AnalysisType = req.AnalysisType,
                            ScoresJsonEncrypted = enc.Encrypt(System.Text.Json.JsonSerializer.Serialize(scoresObj)),
                            MetadataJsonEncrypted = enc.Encrypt(System.Text.Json.JsonSerializer.Serialize(metadata)),
                            CreatedAtUtc = DateTime.UtcNow
                        };
                        db.AnalysisHistories.Add(record);
                        await db.SaveChangesAsync(stoppingToken);

                        // Notify job completed
                        await _hubContext.Clients.All.SendAsync("analysisCompleted", req.SessionId, cancellationToken: stoppingToken);
                    }
                    else
                    {
                        await Task.Delay(250, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in AnalysisWorker loop");
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
    }
}

