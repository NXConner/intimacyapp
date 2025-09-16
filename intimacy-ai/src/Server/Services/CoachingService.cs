using System;
using System.Collections.Generic;

namespace IntimacyAI.Server.Services
{
    public interface ICoachingService
    {
        List<string> GenerateSuggestions(Dictionary<string, object> analysis);
    }

    public sealed class SimpleCoachingService : ICoachingService
    {
        public List<string> GenerateSuggestions(Dictionary<string, object> analysis)
        {
            var suggestions = new List<string>();
            var arousal = TryGetDouble(analysis, "arousal", defaultValue: 0.0);
            var engagement = TryGetDouble(analysis, "engagement", defaultValue: 0.0);

            if (arousal < 0.5)
            {
                suggestions.Add("Increase warm tones or softer lighting to enhance mood.");
            }

            if (engagement < 0.6)
            {
                suggestions.Add("Adjust framing to place the subject along rule-of-thirds lines.");
            }

            suggestions.Add("Avoid cluttered backgrounds to keep attention on the subject.");
            suggestions.Add("Ensure adequate and diffused lighting to reduce harsh shadows.");
            return suggestions;
        }

        private static double TryGetDouble(Dictionary<string, object> dict, string key, double defaultValue)
        {
            if (dict.TryGetValue(key, out var value))
            {
                if (value is double d) return d;
                if (value is float f) return f;
                if (value is int i) return i;
                if (value is long l) return l;
                if (value is string s && double.TryParse(s, out var parsed)) return parsed;
            }
            return defaultValue;
        }
    }
}

