using System;

namespace IntimacyAI.Server.Models
{
    public class ModelPerformance
    {
        public int Id { get; set; }
        public string? ModelVersion { get; set; }
        public string? AccuracyMetricsJson { get; set; }
        public string? PerformanceMetricsJson { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}

