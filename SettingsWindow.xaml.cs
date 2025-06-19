using System.Windows;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Input;

namespace RayCast
{
    public partial class SettingsWindow : Window
    {
        private Settings currentSettings;
        private bool isListeningHotkey = false;

        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            if (File.Exists("config.json"))
            {
                var json = File.ReadAllText("config.json");
                currentSettings = JsonConvert.DeserializeObject<Settings>(json) ?? new Settings();
            }
            else
            {
                currentSettings = new Settings();
            }

            // Moteur de recherche
            SearchEngineComboBox.SelectedItem = null;
            foreach (var item in SearchEngineComboBox.Items)
            {
                if ((item as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() == currentSettings.SearchEngine)
                    SearchEngineComboBox.SelectedItem = item;
            }
            // Thème général
            ThemeComboBox.SelectedItem = null;
            foreach (var item in ThemeComboBox.Items)
            {
                if ((item as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() == currentSettings.Theme)
                    ThemeComboBox.SelectedItem = item;
            }
            // Ordre d'affichage
            OrderComboBox.SelectedItem = null;
            foreach (var item in OrderComboBox.Items)
            {
                if ((item as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() == currentSettings.ResultsOrder)
                    OrderComboBox.SelectedItem = item;
            }
            // Thème IA
            AIThemeComboBox.SelectedItem = null;
            foreach (var item in AIThemeComboBox.Items)
            {
                if ((item as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() == currentSettings.AITheme)
                    AIThemeComboBox.SelectedItem = item;
            }
            // Raccourci
            HotkeyTextBox.Text = currentSettings.Hotkey ?? "";
            EnableHotkeyCheckBox.IsChecked = currentSettings.IsHotkeyEnabled;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Moteur de recherche
            if (SearchEngineComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem searchEngineItem)
                currentSettings.SearchEngine = searchEngineItem.Content.ToString();
            // Thème général
            if (ThemeComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem themeItem)
                currentSettings.Theme = themeItem.Content.ToString();
            // Ordre d'affichage
            if (OrderComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem orderItem)
                currentSettings.ResultsOrder = orderItem.Content.ToString();
            // Thème IA
            if (AIThemeComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem aiThemeItem)
                currentSettings.AITheme = aiThemeItem.Content.ToString();
            // Raccourci
            currentSettings.Hotkey = HotkeyTextBox.Text;
            currentSettings.IsHotkeyEnabled = EnableHotkeyCheckBox.IsChecked == true;

            File.WriteAllText("config.json", JsonConvert.SerializeObject(currentSettings, Formatting.Indented));
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SetHotkeyButton_Click(object sender, RoutedEventArgs e)
        {
            isListeningHotkey = true;
            HotkeyTextBox.Text = "Appuyez sur une touche...";
            HotkeyTextBox.Focus();
        }

        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (isListeningHotkey)
            {
                // Ignore modifieurs seuls
                if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                    e.Key == Key.LeftAlt || e.Key == Key.RightAlt ||
                    e.Key == Key.LeftShift || e.Key == Key.RightShift ||
                    e.Key == Key.LWin || e.Key == Key.RWin)
                {
                    return;
                }
                string modStr = "";
                if ((Keyboard.Modifiers & ModifierKeys.Control) != 0) modStr += "Ctrl+";
                if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0) modStr += "Alt+";
                if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0) modStr += "Shift+";
                if ((Keyboard.Modifiers & ModifierKeys.Windows) != 0) modStr += "Win+";
                HotkeyTextBox.Text = modStr + e.Key.ToString();
                isListeningHotkey = false;
                e.Handled = true;
            }
        }
    }
} 