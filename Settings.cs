using System.Windows.Input;

namespace RayCast
{
    public class Settings
    {
        public string SearchEngine { get; set; } = "Google";
        public string Theme { get; set; } = "Light";
        public string AIEngine { get; set; } = "Gemini";
        public bool IsHotkeyEnabled { get; set; }
<<<<<<< HEAD
        public string Hotkey { get; set; }
        public ModifierKeys Modifiers { get; set; }
        public bool StartupEnabled { get; set; }
        public string? GeminiApiKey { get; set; }
        public string ResultsOrder { get; set; } = "IA,Application,Web";
        public string AITheme { get; set; } = "Light";
=======
        public Key Hotkey { get; set; }
        public ModifierKeys Modifiers { get; set; }
        public bool StartupEnabled { get; set; }
        public string? GeminiApiKey { get; set; }
>>>>>>> b759362d32535175e990742b02ab9f1f12ceaced
    }
} 