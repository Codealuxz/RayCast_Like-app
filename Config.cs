using System;
using System.Text.Json.Serialization;

namespace RayCast
{
    public class Config
    {
        [JsonPropertyName("geminiApiKey")]
        public string GeminiApiKey { get; set; } = string.Empty;

        [JsonPropertyName("theme")]
        public string Theme { get; set; } = "light";

        [JsonPropertyName("startWithWindows")]
        public bool StartWithWindows { get; set; } = true;

        [JsonPropertyName("showInTaskbar")]
        public bool ShowInTaskbar { get; set; } = false;

        [JsonPropertyName("hotkey")]
        public string Hotkey { get; set; } = "Ctrl+Space";

        [JsonPropertyName("exitHotkey")]
        public string ExitHotkey { get; set; } = "Ctrl+C";
    }

    public class GeminiResponse
    {
        public GeminiCandidate[]? candidates { get; set; }
    }

    public class GeminiCandidate
    {
        public GeminiContent? content { get; set; }
    }

    public class GeminiContent
    {
        public GeminiPart[]? parts { get; set; }
    }

    public class GeminiPart
    {
        public string? text { get; set; }
    }
} 