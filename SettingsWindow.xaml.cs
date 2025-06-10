using System;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Media;

namespace RayCast
{
    public partial class SettingsWindow : Window
    {
        private const string REGISTRY_PATH = @"SOFTWARE\RayCast";
        private const string DEFAULT_SEARCH_ENGINE = "Google";
        private const string DEFAULT_THEME = "Light";
        private const string GEMINI_API_KEY = "REPLACE_BY_YOUR_GEMINI_API_KEY";

        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
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
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            System.Windows.MessageBox.Show("Paramètres sauvegardés avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
} 