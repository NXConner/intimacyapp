namespace IntimacyAI.Server.DTOs
{
    public sealed class PreferencesDto
    {
        public string? UserId { get; set; }
        public Dictionary<string, object>? Preferences { get; set; }
    }
}

