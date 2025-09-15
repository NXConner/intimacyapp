using IntimacyAI.Server.Data;
using IntimacyAI.Server.Models;
using IntimacyAI.Server.Hubs;
using IntimacyAI.Server.Security;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

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

                        // Run model inference via abstraction
                        using var scope = _services.CreateScope();
                        var inference = scope.ServiceProvider.GetRequiredService<IModelInferenceService>();
                        var inf = await inference.AnalyzeImageAsync(req.Data, req.Metadata, stoppingToken);

                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var enc = scope.ServiceProvider.GetRequiredService<IEncryptionService>();

                        var record = new AnalysisHistory
                        {
                            SessionId = req.SessionId,
                            AnalysisType = req.AnalysisType,
                            ScoresJsonEncrypted = enc.Encrypt(System.Text.Json.JsonSerializer.Serialize(inf.Scores)),
                            MetadataJsonEncrypted = enc.Encrypt(System.Text.Json.JsonSerializer.Serialize(inf.Metadata)),
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

