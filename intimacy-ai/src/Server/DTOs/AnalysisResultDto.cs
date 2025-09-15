using System;
using System.Collections.Generic;

namespace IntimacyAI.Server.DTOs
{
    public sealed class AnalysisResultDto
    {
        public string? SessionId { get; set; }
        public string? AnalysisType { get; set; }
        public Dictionary<string, object>? Scores { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}

