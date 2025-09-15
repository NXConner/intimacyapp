using System;

namespace IntimacyAI.Server.Models
{
    public class UserPreferences
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? PreferencesJsonEncrypted { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public class AnalysisHistory
    {
        public int Id { get; set; }
        public string? SessionId { get; set; }
        public string? AnalysisType { get; set; }
        public string? ScoresJsonEncrypted { get; set; }
        public string? MetadataJsonEncrypted { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public class CoachingSession
    {
        public int Id { get; set; }
        public string? SessionId { get; set; }
        public string? SuggestionsJsonEncrypted { get; set; }
        public string? FeedbackJsonEncrypted { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}

