using System;

namespace IntimacyAI.Server.Models
{
    public class UsageAnalytics
    {
        public int Id { get; set; }
        public string? AnonymousUserId { get; set; }
        public string? FeatureUsed { get; set; }
        public int? UsageDurationSeconds { get; set; }
        public string? Platform { get; set; }
        public string? AppVersion { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}

