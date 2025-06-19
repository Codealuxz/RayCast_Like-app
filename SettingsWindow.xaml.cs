<<<<<<< HEAD
using System.Windows;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Input;
=======
using System;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Media;
>>>>>>> b759362d32535175e990742b02ab9f1f12ceaced

namespace RayCast
{
    public partial class SettingsWindow : Window
    {
<<<<<<< HEAD
        private Settings currentSettings;
        private bool isListeningHotkey = false;
=======
        private const string REGISTRY_PATH = @"SOFTWARE\RayCast";
        private const string DEFAULT_SEARCH_ENGINE = "Google";
        private const string DEFAULT_THEME = "Light";
        private const string GEMINI_API_KEY = "REPLACE_BY_YOUR_GEMINI_API_KEY";
>>>>>>> b759362d32535175e990742b02ab9f1f12ceaced

        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
<<<<<<< HEAD
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
=======
            using (var key = Registry.CurrentUser.CreateSubKey(REGISTRY_PATH))
            {
                if (key != null)
                {
                    
                    var theme = key.GetValue("Theme", DEFAULT_THEME) as string ?? DEFAULT_THEME;
                    var searchEngine = key.GetValue("SearchEngine", DEFAULT_SEARCH_ENGINE) as string ?? DEFAULT_SEARCH_ENGINE;

                    
                    foreach (ComboBoxItem item in ThemeComboBox.Items)
                    {
                        if (item.Content.ToString() == theme)
                        {
                            ThemeComboBox.SelectedItem = item;
                            break;
                        }
                    }

                    foreach (ComboBoxItem item in SearchEngineComboBox.Items)
                    {
                        if (item.Content.ToString() == searchEngine)
                        {
                            SearchEngineComboBox.SelectedItem = item;
                            break;
                        }
                    }

                    EnableHotkeyCheckBox.IsChecked = Convert.ToBoolean(key.GetValue("EnableHotkey", false));
                    HotkeyTextBox.Text = key.GetValue("Hotkey", "Ctrl+Space") as string ?? "Ctrl+Space";
                    StartupCheckBox.IsChecked = Convert.ToBoolean(key.GetValue("Startup", false));

                    
                    key.SetValue("GeminiApiKey", GEMINI_API_KEY);
                }
            }
        }

        private void SaveSettings()
        {
            using (var key = Registry.CurrentUser.CreateSubKey(REGISTRY_PATH))
            {
                if (key != null)
                {
                    
                    key.SetValue("Theme", (ThemeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? DEFAULT_THEME);
                    key.SetValue("SearchEngine", (SearchEngineComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? DEFAULT_SEARCH_ENGINE);
                    key.SetValue("EnableHotkey", EnableHotkeyCheckBox.IsChecked ?? false);
                    key.SetValue("Hotkey", HotkeyTextBox.Text);
                    key.SetValue("Startup", StartupCheckBox.IsChecked ?? false);
                    key.SetValue("GeminiApiKey", GEMINI_API_KEY);

                    
                    SetStartup(StartupCheckBox.IsChecked ?? false);

                    
                    ApplyTheme((ThemeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? DEFAULT_THEME);
                }
            }
        }

        private void SetStartup(bool enable)
        {
            using (var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (enable)
                {
                    key?.SetValue("RayCast", System.Reflection.Assembly.GetExecutingAssembly().Location);
                }
                else
                {
                    key?.DeleteValue("RayCast", false);
                }
            }
        }

        private void ApplyTheme(string theme)
        {
            var app = System.Windows.Application.Current;
            if (app != null)
            {
                switch (theme)
                {
                    case "Dark":
                        app.Resources["BackgroundColor"] = app.Resources["DarkBackgroundColor"];
                        app.Resources["ForegroundColor"] = app.Resources["DarkForegroundColor"];
                        app.Resources["BorderColor"] = app.Resources["DarkBorderColor"];
                        app.Resources["SelectionColor"] = app.Resources["DarkSelectionColor"];
                        app.Resources["HoverColor"] = app.Resources["DarkHoverColor"];
                        break;
                    case "Light":
                        app.Resources["BackgroundColor"] = new SolidColorBrush(Colors.White);
                        app.Resources["ForegroundColor"] = new SolidColorBrush(Colors.Black);
                        app.Resources["BorderColor"] = new SolidColorBrush(Colors.LightGray);
                        app.Resources["SelectionColor"] = new SolidColorBrush(Colors.LightBlue);
                        app.Resources["HoverColor"] = new SolidColorBrush(Colors.LightGray);
                        break;
                    case "System":
                        var isDark = System.Windows.Forms.SystemInformation.HighContrast;
                        if (isDark)
                        {
                            app.Resources["BackgroundColor"] = app.Resources["DarkBackgroundColor"];
                            app.Resources["ForegroundColor"] = app.Resources["DarkForegroundColor"];
                            app.Resources["BorderColor"] = app.Resources["DarkBorderColor"];
                            app.Resources["SelectionColor"] = app.Resources["DarkSelectionColor"];
                            app.Resources["HoverColor"] = app.Resources["DarkHoverColor"];
                        }
                        else
                        {
                            app.Resources["BackgroundColor"] = new SolidColorBrush(Colors.White);
                            app.Resources["ForegroundColor"] = new SolidColorBrush(Colors.Black);
                            app.Resources["BorderColor"] = new SolidColorBrush(Colors.LightGray);
                            app.Resources["SelectionColor"] = new SolidColorBrush(Colors.LightBlue);
                            app.Resources["HoverColor"] = new SolidColorBrush(Colors.LightGray);
                        }
                        break;
                }

                
                app.Resources.MergedDictionaries.Clear();
                app.Resources.MergedDictionaries.Add(new ResourceDictionary());

                
                foreach (Window window in app.Windows)
                {
                    if (window is MainWindow mainWindow)
                    {
                        mainWindow.UpdateTheme();
                    }
                }
            }
>>>>>>> b759362d32535175e990742b02ab9f1f12ceaced
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
<<<<<<< HEAD
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
=======
            SaveSettings();
            System.Windows.MessageBox.Show("Paramètres sauvegardés avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
>>>>>>> b759362d32535175e990742b02ab9f1f12ceaced
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
<<<<<<< HEAD
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
=======
            Close();
>>>>>>> b759362d32535175e990742b02ab9f1f12ceaced
        }
    }
} 