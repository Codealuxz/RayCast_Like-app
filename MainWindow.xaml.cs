using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Microsoft.Win32;
using System.Net.Http.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Data;
using Windows.Management.Deployment;
using Newtonsoft.Json;
using System.Windows.Documents;
using System.Management;

namespace RayCast
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly ObservableCollection<SearchResult> searchResults = new();
        private NotifyIcon? _notifyIcon;
        private KeyboardHook? _keyboardHook;
        private string geminiApiKey = string.Empty;
        private HotKey? _hotKey;
        private HotKey? _exitHotKey;
        private HwndSource? _source;
        private List<AppInfo> installedApps = new();
        private bool _isExiting = false;
        private bool _isIAActive = false;
        private System.Threading.Timer? _searchTimer;
        private string _lastSearchText = string.Empty;
        private bool _isGenerating = false;
        private const string CACHE_FILE = "exe_cache.json";
        private DateTime lastCacheUpdate = DateTime.MinValue;
        private DispatcherTimer _searchDelayTimer;
        private string logFilePath;
        private Settings? currentSettings;
        private Scanner scanner;

        public bool IsIAActive
        {
            get => _isIAActive;
            set
            {
                _isIAActive = value;
                OnPropertyChanged(nameof(IsIAActive));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            try
            {
                SafeLog("Début de l'initialisation de MainWindow.");
                InitializeComponent();
                SafeLog("InitializeComponent terminé.");

                
                string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RayCast");
                Directory.CreateDirectory(appDataPath);
                logFilePath = Path.Combine(appDataPath, "raycast.log");
                SafeLog($"Chemins de fichiers initialisés : logFilePath={logFilePath}");

                
                InitializeSystemTray();
                SafeLog("Systray initialisée.");

                
                SafeLog("Initialisation de MainWindow terminée.");

                Loaded += MainWindow_Loaded;
                Closing += MainWindow_Closing;
                KeyDown += MainWindow_KeyDown;
                Deactivated += MainWindow_Deactivated;

                
                Show();
                Hide();

                ResultsList.ItemsSource = searchResults;
                ResultsList.SelectionChanged += ResultsList_SelectionChanged;
                ResultsList.MouseDoubleClick += ResultsList_MouseDoubleClick;

                LoadInstalledApps();
                SetupWindowEvents();

                _searchDelayTimer = new DispatcherTimer();
                _searchDelayTimer.Interval = TimeSpan.FromSeconds(2);
                _searchDelayTimer.Tick += SearchDelayTimer_Tick;

                LoadSettings();

                scanner = new Scanner();
                // Scan combiné : API Windows Search + raccourcis utilisateur
                Task.Run(() =>
                {
                    scanner.ScanWithWindowsSearch();
                    scanner.ScanUserShortcuts();
                    scanner.FilterAndDeduplicate();
                    scanner.SaveCache();
                });
            }
            catch (Exception ex)
            {
                SafeLog($"Erreur lors de l'initialisation de MainWindow : {ex.Message}");
                throw;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                SafeLog("Enregistrement des raccourcis...");
                var handle = new WindowInteropHelper(this).Handle;
                
                
                _source = HwndSource.FromHwnd(handle);
                _source?.AddHook(WndProc);
                
                
                RegisterUserHotkey();
                
                
                SafeLog("Raccourcis enregistrés avec succès !");

                
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = false;
                    _notifyIcon.Dispose();
                }

                
                _notifyIcon = new NotifyIcon();
                string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.ico");
                
                if (System.IO.File.Exists(iconPath))
                {
                    try
                    {
                        _notifyIcon.Icon = new System.Drawing.Icon(iconPath);
                        SafeLog("Icône logo.ico chargée avec succès.");
                    }
                    catch (Exception iconEx)
                    {
                        SafeLog($"Erreur lors du chargement de l'icône : {iconEx.Message}");
                        _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
                    }
                }
                else
                {
                    SafeLog($"Le fichier d'icône logo.ico n'a pas été trouvé à {iconPath}. Utilisation de l'icône par défaut.");
                    _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
                }

                _notifyIcon.Visible = true;
                _notifyIcon.Text = "RayCast";
                _notifyIcon.BalloonTipTitle = "RayCast";
                _notifyIcon.BalloonTipText = "RayCast est lancé !";
                _notifyIcon.ShowBalloonTip(2000);
                _notifyIcon.DoubleClick += (s, args) => ShowWindow(s, args);

                
                var contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("Ouvrir", null, (s, e) => ShowWindow(s, e));
                contextMenu.Items.Add("Paramètres", null, (s, e) => {
                    var settingsWindow = new SettingsWindow();
                    settingsWindow.Closed += (s, args) =>
                    {
                        LoadSettings();
                        ApplyTheme(currentSettings?.Theme ?? "Light");
                        RegisterUserHotkey();
                        if (!string.IsNullOrEmpty(SearchBox.Text))
                        {
                            _ = UpdateNormalSearch(SearchBox.Text);
                        }
                    };
                    settingsWindow.Show();
                });
                contextMenu.Items.Add("Quitter", null, (s, e) => ExitApplication(s, e));
                _notifyIcon.ContextMenuStrip = contextMenu;

                
                Task.Run(() => UpdateExeCache());
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de l'enregistrement des raccourcis ou de la systray : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                SafeLog($"Erreur globale dans MainWindow_Loaded : {ex.Message}");
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY)
            {
                SafeLog("Message WM_HOTKEY reçu.");
                int id = wParam.ToInt32();
                if (id == _hotKey?.GetHashCode())
                {
                    _hotKey.ProcessHotKey();
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }
            e.Cancel = true;
            HideWindow();
        }

        private void HotKey_Pressed(object? sender, EventArgs e)
        {
            
            try
            {
                SafeLog("Raccourci Ctrl+Espace pressé (HotKey_Pressed déclenché).");
            }
            catch {}
            SafeLog("Raccourci Ctrl + Espace détecté ! Affichage de la fenêtre.");
            ShowWindow(sender, null);
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    HideWindow();
                    e.Handled = true;
                    break;
                case Key.Enter:
                    if (ResultsList.SelectedItem is SearchResult selectedResult)
                    {
                        ExecuteAction(selectedResult);
                        if (selectedResult.ActionType != "ia")
                        {
                            HideWindow();
                        }
                        e.Handled = true;
                    }
                    else if (ResultsList.Items.Count > 0)
                    {
                        ResultsList.SelectedIndex = 0;
                        if (ResultsList.SelectedItem is SearchResult firstResult)
                        {
                            ExecuteAction(firstResult);
                            if (firstResult.ActionType != "ia")
                            {
                                HideWindow();
                            }
                        }
                        e.Handled = true;
                    }
                    break;
                case Key.Down:
                    if (ResultsList.Items.Count > 0)
                    {
                        ResultsList.SelectedIndex = ResultsList.SelectedIndex < 0 ? 0 : Math.Min(ResultsList.SelectedIndex + 1, ResultsList.Items.Count - 1);
                        ResultsList.ScrollIntoView(ResultsList.SelectedItem);
                        e.Handled = true;
                    }
                    break;
                case Key.Up:
                    if (ResultsList.Items.Count > 0)
                    {
                        ResultsList.SelectedIndex = ResultsList.SelectedIndex <= 0 ? ResultsList.Items.Count - 1 : ResultsList.SelectedIndex - 1;
                        ResultsList.ScrollIntoView(ResultsList.SelectedItem);
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void MainWindow_Deactivated(object? sender, EventArgs e)
        {
            HideWindow();
        }

        private void ShowWindow(object? sender, EventArgs e)
        {
            Show();
            Activate();
            SearchBox.Focus();
        }

        private void HideWindow()
        {
            Hide();
            SearchBox.Text = ""; 
            searchResults.Clear(); 
            IsIAActive = false; 
            IAResponseBox.Document.Blocks.Clear(); 
            IAResponseBox.Visibility = Visibility.Collapsed; 
            SafeLog("Fenêtre masquée.");
        }

        private void ResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            SafeLog($"Sélection changée : {ResultsList.SelectedIndex}");
        }

        private void ResultsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = ResultsList.SelectedItem as SearchResult;
            if (selectedItem != null)
            {
                ExecuteAction(selectedItem);
            }
        }

        private async void ExecuteAction(SearchResult result)
        {
            switch (result.ActionType)
            {
                case "app":
                    if (!string.IsNullOrEmpty(result.AppPath))
                        LaunchApplication(result.AppPath);
                    break;
                case "web":
                    if (!string.IsNullOrEmpty(result.WebUrl))
                        OpenWebSearch(result.WebUrl);
                    else
                        OpenWebSearch(result.Title);
                    break;
                case "terminal":
                    ExecuteTerminalCommand(result.Title);
                    break;
                case "copy":
                    System.Windows.Clipboard.SetText(result.Title);
                    break;
                case "ia":
                    ActivateIA(result.Title);
                    break;
                case "generate_ia":
                    if (_isGenerating) return;
                    _isGenerating = true;
                    try
                    {
                        
                        searchResults.Clear();
                        
                        IsIAActive = true;
                        
                        SearchBox.Text = "";

                        var suggestion = await GetGeminiSuggestion(SearchBox.Text);
                        if (!string.IsNullOrEmpty(suggestion))
                        {
                            
                            await Task.Delay(500);
                            SearchBox.Text = suggestion;
                        }
                        else
                        {
                            SearchBox.Text = "Impossible de générer une réponse";
                        }
                    }
                    catch (Exception ex)
                    {
                        SearchBox.Text = $"Erreur : {ex.Message}";
                    }
                    finally
                    {
                        _isGenerating = false;
                    }
                    break;
            }
        }

        private void OpenWebSearch(string url)
        {
            try
            {
                if (url.StartsWith("http://") || url.StartsWith("https://"))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                else
                {
                    string searchEngine;
                    using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\RayCast"))
                    {
                        searchEngine = key?.GetValue("SearchEngine") as string ?? "Google";
                    }

                    string searchUrl;
                    switch (searchEngine)
                    {
                        case "Ecosia":
                            searchUrl = $"https://www.ecosia.org/search?q={Uri.EscapeDataString(url)}";
                            break;
                        case "Bing":
                            searchUrl = $"https://www.bing.com/search?q={Uri.EscapeDataString(url)}";
                            break;
                        case "DuckDuckGo":
                            searchUrl = $"https://duckduckgo.com/?q={Uri.EscapeDataString(url)}";
                            break;
                        case "Qwant":
                            searchUrl = $"https://www.qwant.com/?q={Uri.EscapeDataString(url)}";
                            break;
                        case "Google":
                        default:
                            searchUrl = $"https://www.google.com/search?q={Uri.EscapeDataString(url)}";
                            break;
                    }
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = searchUrl,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de l'ouverture du lien : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LaunchApplication(string appPath)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = appPath,
                    UseShellExecute = true
                });
                SafeLog($"Application lancée : {appPath}");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors du lancement de {appPath}: {ex.Message}");
                SafeLog($"Erreur lors du lancement de {appPath}: {ex.Message}");
            }
        }

        private void ExecuteTerminalCommand(string command)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/k {command}",
                    UseShellExecute = true,
                    CreateNoWindow = true
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de l'exécution de la commande : {ex.Message}");
            }
        }

        private void LoadInstalledApps()
        {
            try
            {
                installedApps.Clear();
                
                
                var registryKeys = new[]
                {
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                    @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall",
                    @"SOFTWARE\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Packages"
                };

                foreach (var keyPath in registryKeys)
                {
                    using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath);
                    if (key != null)
                    {
                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            try
                            {
                                using var subKey = key.OpenSubKey(subKeyName);
                                if (subKey != null)
                                {
                                    var displayName = subKey.GetValue("DisplayName")?.ToString();
                                    var installLocation = subKey.GetValue("InstallLocation")?.ToString();
                                    var displayIcon = subKey.GetValue("DisplayIcon")?.ToString();
                                    var publisher = subKey.GetValue("Publisher")?.ToString();

                                    if (!string.IsNullOrEmpty(displayName))
                                    {
                                        string? exePath = null;
                                        if (!string.IsNullOrEmpty(installLocation))
                                        {
                                            
                                            installLocation = installLocation.Trim('"');
                                            if (Directory.Exists(installLocation))
                                            {
                                                try
                                                {
                                                    var exeFiles = Directory.GetFiles(installLocation, "*.exe", SearchOption.TopDirectoryOnly);
                                                    if (exeFiles.Length > 0)
                                                    {
                                                        exePath = exeFiles.FirstOrDefault(f => 
                                                            Path.GetFileNameWithoutExtension(f).ToLower() == displayName.ToLower() ||
                                                            f.Contains(displayName.Split(' ')[0])) ?? exeFiles[0];
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    SafeLog($"Erreur lors de la recherche dans {installLocation}: {ex.Message}");
                                                }
                                            }
                                        }

                                        if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                                        {
                                            installedApps.Add(new AppInfo
                                            {
                                                Name = displayName,
                                                Path = exePath,
                                                DisplayName = displayName,
                                                Icon = "📱"
                                            });
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                SafeLog($"Erreur lors du chargement de l'application {subKeyName}: {ex.Message}");
                            }
                        }
                    }
                }

                
                var programFilesPaths = new[]
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
                };

                foreach (var programFilesPath in programFilesPaths)
                {
                    if (Directory.Exists(programFilesPath))
                    {
                        try
                        {
                            var exeFiles = Directory.GetFiles(programFilesPath, "*.exe", SearchOption.AllDirectories)
                                .Where(f => !f.Contains("\\Windows\\") && 
                                          !f.Contains("\\Microsoft\\") && 
                                          !f.Contains("\\Common Files\\") &&
                                          !f.Contains("\\Fichiers communs\\"));

                            foreach (var exeFile in exeFiles)
                            {
                                try
                                {
                                    var fileInfo = FileVersionInfo.GetVersionInfo(exeFile);
                                    var displayName = !string.IsNullOrEmpty(fileInfo.FileDescription) 
                                        ? fileInfo.FileDescription 
                                        : Path.GetFileNameWithoutExtension(exeFile);

                                    if (!installedApps.Any(a => a.Path.Equals(exeFile, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        installedApps.Add(new AppInfo
                                        {
                                            Name = displayName,
                                            Path = exeFile,
                                            DisplayName = displayName,
                                            Icon = "💻"
                                        });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    SafeLog($"Erreur lors du chargement de l'exécutable {exeFile}: {ex.Message}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SafeLog($"Erreur lors de la recherche dans {programFilesPath}: {ex.Message}");
                        }
                    }
                }

                
                var systemApps = new Dictionary<string, string>
                {
                    { "Calculatrice", "calc.exe" },
                    { "Bloc-notes", "notepad.exe" },
                    { "Paint", "mspaint.exe" },
                    { "Invite de commandes", "cmd.exe" },
                    { "PowerShell", "powershell.exe" },
                    { "Explorateur Windows", "explorer.exe" },
                    { "Éditeur du Registre", "regedit.exe" },
                    { "Gestionnaire des tâches", "taskmgr.exe" },
                    { "Panneau de configuration", "control.exe" },
                    { "Configuration système", "msconfig.exe" },
                    { "Services", "services.msc" },
                    { "Gestion des disques", "diskmgmt.msc" }
                };

                foreach (var app in systemApps)
                {
                    installedApps.Add(new AppInfo
                    {
                        Name = app.Key,
                        Path = app.Value,
                        DisplayName = app.Key,
                        Icon = "💻"
                    });
                }

                
                var startMenuPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                    "Programs"
                );
                var userStartMenuPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                    "Programs"
                );

                var searchPaths = new[] { startMenuPath, userStartMenuPath };
                foreach (var path in searchPaths)
                {
                    if (Directory.Exists(path))
                    {
                        var shortcuts = Directory.GetFiles(path, "*.lnk", SearchOption.AllDirectories);
                        foreach (var shortcut in shortcuts)
                        {
                            try
                            {
                                var targetPath = GetShortcutTarget(shortcut);
                                if (!string.IsNullOrEmpty(targetPath) && File.Exists(targetPath))
                                {
                                    var displayName = Path.GetFileNameWithoutExtension(shortcut);
                                    if (!installedApps.Any(a => a.Path.Equals(targetPath, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        installedApps.Add(new AppInfo
                                        {
                                            Name = displayName,
                                            Path = targetPath,
                                            DisplayName = displayName,
                                            Icon = "📱"
                                        });
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                SafeLog($"Erreur lors du chargement du raccourci {shortcut}: {ex.Message}");
                            }
                        }
                    }
                }

                
                if (installedApps.Count == 0)
                {
                    SafeLog("Aucune application trouvée, lancement de la recherche de secours...");
                    SearchAllExecutables();
                }

                
                installedApps = installedApps
                    .GroupBy(a => a.Path.ToLower())
                    .Select(g => g.First())
                    .OrderBy(a => a.DisplayName)
                    .ToList();

                SafeLog($"{installedApps.Count} applications trouvées.");
            }
            catch (Exception ex)
            {
                SafeLog($"Erreur lors du chargement des applications : {ex.Message}");
            }
        }

        private string GetShortcutTarget(string shortcutPath)
        {
            try
            {
                using var fileStream = new FileStream(shortcutPath, FileMode.Open, FileAccess.Read);
                using var reader = new BinaryReader(fileStream);

                
                fileStream.Seek(0x4C, SeekOrigin.Begin);
                var flags = reader.ReadInt32();
                fileStream.Seek(0x60, SeekOrigin.Begin);
                var fileOffset = reader.ReadInt32();

                
                fileStream.Seek(fileOffset, SeekOrigin.Begin);
                var pathLength = reader.ReadInt16();
                var pathBytes = reader.ReadBytes(pathLength * 2);
                var path = System.Text.Encoding.Unicode.GetString(pathBytes);

                return path.TrimEnd('\0');
            }
            catch
            {
                return string.Empty;
            }
        }

        private void SearchAllExecutables()
        {
            try
            {
                if (!File.Exists(CACHE_FILE))
                {
                    
                    UpdateExeCache();
                }

                
                var json = File.ReadAllText(CACHE_FILE);
                var exeList = JsonConvert.DeserializeObject<List<ExeCacheEntry>>(json);

                if (exeList != null)
                {
                    foreach (var exe in exeList)
                    {
                        try
                        {
                            
                            if (File.Exists(exe.Path))
                            {
                                
                                var currentLastModified = File.GetLastWriteTime(exe.Path);
                                if (currentLastModified > exe.LastModified)
                                {
                                    
                                    var fileInfo = FileVersionInfo.GetVersionInfo(exe.Path);
                                    exe.DisplayName = !string.IsNullOrEmpty(fileInfo.FileDescription) 
                                        ? fileInfo.FileDescription 
                                        : Path.GetFileNameWithoutExtension(exe.Path);
                                    exe.LastModified = currentLastModified;
                                }

                                if (!installedApps.Any(a => a.Path.Equals(exe.Path, StringComparison.OrdinalIgnoreCase)))
                                {
                                    installedApps.Add(new AppInfo
                                    {
                                        Name = exe.DisplayName,
                                        Path = exe.Path,
                                        DisplayName = exe.DisplayName,
                                        Icon = "💻"
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SafeLog($"Erreur lors du chargement de l'exécutable {exe.Path}: {ex.Message}");
                        }
                    }

                    
                    var updatedJson = JsonConvert.SerializeObject(exeList);
                    File.WriteAllText(CACHE_FILE, updatedJson);
                }
            }
            catch (Exception ex)
            {
                SafeLog("Erreur lors de la recherche de secours : " + ex.Message);
            }
        }

        private void UpdateExeCache()
        {
            try
            {
                SafeLog("Début de la mise à jour du cache...");
                var exeList = new List<ExeCacheEntry>();

                // Recherche dans Program Files et Program Files (x86)
                var programFilesPaths = new[]
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
                };

                foreach (var programFilesPath in programFilesPaths)
                {
                    if (Directory.Exists(programFilesPath))
                    {
                        try
                        {
                            SafeLog($"Recherche dans {programFilesPath}...");
                            var exeFiles = Directory.GetFiles(programFilesPath, "*.exe", SearchOption.AllDirectories)
                                .Where(f => !f.Contains("\\Windows\\") && 
                                          !f.Contains("\\Microsoft\\") && 
                                          !f.Contains("\\Common Files\\") &&
                                          !f.Contains("\\Fichiers communs\\"));

                            foreach (var exeFile in exeFiles)
                            {
                                try
                                {
                                    var fileInfo = FileVersionInfo.GetVersionInfo(exeFile);
                                    var displayName = !string.IsNullOrEmpty(fileInfo.FileDescription) 
                                        ? fileInfo.FileDescription 
                                        : Path.GetFileNameWithoutExtension(exeFile);

                                    exeList.Add(new ExeCacheEntry
                                    {
                                        Path = exeFile,
                                        DisplayName = displayName,
                                        LastModified = File.GetLastWriteTime(exeFile)
                                    });
                                }
                                catch (Exception ex)
                                {
                                    SafeLog($"Erreur lors du chargement de {exeFile}: {ex.Message}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SafeLog($"Erreur lors de la recherche dans {programFilesPath}: {ex.Message}");
                        }
                    }
                }

                // Recherche dans le dossier utilisateur
                var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var userFolders = new[]
                {
                    Path.Combine(userProfile, "AppData", "Local"),
                    Path.Combine(userProfile, "AppData", "Roaming"),
                    Path.Combine(userProfile, "Downloads"),
                    Path.Combine(userProfile, "Desktop")
                };

                foreach (var folder in userFolders)
                {
                    if (Directory.Exists(folder))
                    {
                        try
                        {
                            SafeLog($"Recherche dans {folder}...");
                            var exeFiles = Directory.GetFiles(folder, "*.exe", SearchOption.AllDirectories);
                            foreach (var exeFile in exeFiles)
                            {
                                try
                                {
                                    var fileInfo = FileVersionInfo.GetVersionInfo(exeFile);
                                    var displayName = !string.IsNullOrEmpty(fileInfo.FileDescription) 
                                        ? fileInfo.FileDescription 
                                        : Path.GetFileNameWithoutExtension(exeFile);

                                    exeList.Add(new ExeCacheEntry
                                    {
                                        Path = exeFile,
                                        DisplayName = displayName,
                                        LastModified = File.GetLastWriteTime(exeFile)
                                    });
                                }
                                catch (Exception ex)
                                {
                                    SafeLog($"Erreur lors du chargement de {exeFile}: {ex.Message}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SafeLog($"Erreur lors de la recherche dans {folder}: {ex.Message}");
                        }
                    }
                }

                // Sauvegarder le cache
                var json = JsonConvert.SerializeObject(exeList);
                File.WriteAllText(CACHE_FILE, json);
                SafeLog("Cache mis à jour avec succès.");
            }
            catch (Exception ex)
            {
                SafeLog("Erreur lors de la mise à jour du cache : " + ex.Message);
            }
        }

        private class ExeCacheEntry
        {
            public string Path { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public DateTime LastModified { get; set; }
        }

        private async Task<string> GetGeminiSuggestion(string query)
        {
            try
            {
                SafeLog("Début de la génération IA pour : " + query);
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("x-goog-api-key", geminiApiKey);

                var request = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[]
                            {
                                new { text = query }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.9,
                        topK = 40,
                        topP = 0.95,
                        maxOutputTokens = 2048
                    }
                };

                SafeLog("Envoi de la requête à Gemini...");
                var response = await client.PostAsJsonAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={geminiApiKey}",
                    request);

                SafeLog("Réponse reçue, status: " + response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(stream);
                    var fullResponse = new StringBuilder();

                    SafeLog("Début de la lecture du stream");

                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        if (string.IsNullOrEmpty(line)) continue;

                        if (line.StartsWith("data: "))
                        {
                            var jsonData = line.Substring(6);
                            if (jsonData == "[DONE]") 
                            {
                                SafeLog("Stream terminé");
                                break;
                            }

                            try
                            {
                                var result = JsonConvert.DeserializeObject<GeminiStreamResponse>(jsonData);
                                if (result?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text != null)
                                {
                                    var text = result.candidates.First().content.parts.First().text;
                                    fullResponse.Append(text);
                                    
                                    SafeLog("Nouveau texte reçu : " + text);
                                    
                                    
                                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                                    {
                                        var currentResult = searchResults.FirstOrDefault(r => r.ActionType == "web");
                                        if (currentResult != null)
                                        {
                                            currentResult.Title = fullResponse.ToString();
                                            SafeLog("Interface mise à jour avec : " + fullResponse);
                                        }
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                SafeLog("Erreur parsing JSON : " + ex.Message);
                            }
                        }
                    }

                    var finalResponse = fullResponse.ToString();
                    SafeLog("Réponse finale : " + finalResponse);
                    return finalResponse;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    SafeLog("Erreur API : " + errorContent);
                }
            }
            catch (Exception ex)
            {
                SafeLog("Erreur Gemini : " + ex.Message + "\n" + ex.StackTrace);
            }
            return string.Empty;
        }

        public class GeminiStreamResponse
        {
            public GeminiCandidate[]? candidates { get; set; }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SearchBox.Text))
            {
                _lastSearchText = SearchBox.Text;
                
                
                _ = UpdateNormalSearch(SearchBox.Text);

                
                _searchDelayTimer.Stop();
                _searchDelayTimer.Start();
            }
            else
            {
                _lastSearchText = string.Empty;
                searchResults.Clear();
                ResultsList.Visibility = Visibility.Collapsed;
            }
        }

        private async Task UpdateNormalSearch(string searchText)
        {
            if (_isIAActive) return;

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                searchResults.Clear();
            });

            var searchLower = searchText.ToLower();

            // Préparation des listes par type
            var iaResults = new List<SearchResult>();
            var appResults = new List<SearchResult>();
            var webResults = new List<SearchResult>();

            // Recherche d'applications
            foreach (var app in installedApps)
            {
                if (app.DisplayName.ToLower().Contains(searchLower) || app.Name.ToLower().Contains(searchLower))
                {
                    var appIcon = GetAppIcon(app.Path);
                    appResults.Add(new SearchResult
                    {
                        Icon = app.Icon,
                        Title = "[Application] " + app.DisplayName,
                        Description = app.Path,
                        ActionType = "app",
                        AppPath = app.Path,
                        AppIcon = appIcon
                    });
                }
            }

            // Recherche Web
            string searchEngine = currentSettings?.SearchEngine ?? "Google";
            string searchUrl;
            switch (searchEngine)
            {
                case "Ecosia":
                    searchUrl = $"https://www.ecosia.org/search?q={Uri.EscapeDataString(searchText)}";
                    break;
                case "Bing":
                    searchUrl = $"https://www.bing.com/search?q={Uri.EscapeDataString(searchText)}";
                    break;
                case "DuckDuckGo":
                    searchUrl = $"https://duckduckgo.com/?q={Uri.EscapeDataString(searchText)}";
                    break;
                case "Qwant":
                    searchUrl = $"https://www.qwant.com/?q={Uri.EscapeDataString(searchText)}";
                    break;
                default:
                    searchUrl = $"https://www.google.com/search?q={Uri.EscapeDataString(searchText)}";
                    break;
            }
            webResults.Add(new SearchResult
            {
                Icon = "🌐",
                Title = "[Web] " + searchText,
                Description = "Rechercher sur le web",
                ActionType = "web",
                WebUrl = searchUrl
            });

            // Résultat IA (bouton)
            iaResults.Add(new SearchResult
            {
                Icon = "🤖",
                Title = "[IA] " + searchText,
                Description = "Générer avec l'IA",
                ActionType = "ia"
            });

            // À la fin, ajoute dans searchResults selon l'ordre choisi :
            string resultsOrder = currentSettings?.ResultsOrder ?? "IA,Application,Web";
            foreach (var type in resultsOrder.Split(','))
            {
                switch (type.Trim().ToLower())
                {
                    case "ia":
                        foreach (var r in iaResults) await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => searchResults.Add(r));
                        break;
                    case "application":
                        foreach (var r in appResults) await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => searchResults.Add(r));
                        break;
                    case "web":
                        foreach (var r in webResults) await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => searchResults.Add(r));
                        break;
                }
            }
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ResultsList.Visibility = searchResults.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        private async Task UpdateIASearch(string searchText)
        {
            try
            {
                SafeLog("Début de la génération IA pour : " + searchText);
                
                
                string suggestion = await GetGeminiSuggestion(searchText);
                
                
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    searchResults.Clear();
                    MarkdownConverter.ConvertToRichText(IAResponseBox, suggestion);
                    ResultsList.Visibility = Visibility.Collapsed;
                    IAResponseBox.Visibility = Visibility.Visible;
                });

                SafeLog("Génération IA terminée avec succès.");
            }
            catch (Exception ex)
            {
                SafeLog("Erreur lors de la génération IA : " + ex.Message);
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    searchResults.Clear();
                    MarkdownConverter.ConvertToRichText(IAResponseBox, $"Erreur : {ex.Message}");
                    ResultsList.Visibility = Visibility.Collapsed;
                    IAResponseBox.Visibility = Visibility.Visible;
                });
            }
        }

        private async Task UpdateSearchResults(string searchText)
        {
            searchResults.Clear();

            if (string.IsNullOrWhiteSpace(searchText))
                return;

            if (_isIAActive)
            {
                await UpdateIASearch(searchText);
            }
            else
            {
                await UpdateNormalSearch(searchText);
            }
        }

        private bool IsMathExpression(string text)
        {
            
            text = text.Replace(" ", "");




            text = text.TrimEnd('=');

            
            return text.Contains("+") || text.Contains("-") || text.Contains("*") || 
                   text.Contains("/") || text.Contains(":") || text.Contains("×") || 
                   text.Contains("÷") || text.Contains("^") || text.Contains("√");
        }

        private string CalculateExpression(string expression)
        {
            try
            {
                
                expression = expression.Replace("×", "*")
                                      .Replace("÷", "/")
                                    .Replace(":", "/")
                                    .Replace("²", "^2")
                                    .Replace("³", "^3")
                                    .Replace(" ", "");

                
                expression = expression.TrimEnd('=');

                
                while (expression.Contains("^"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(expression, @"(\d+)\^(\d+)");
                    if (match.Success)
                    {
                        var baseNum = double.Parse(match.Groups[1].Value);
                        var exponent = double.Parse(match.Groups[2].Value);
                        var powResult = Math.Pow(baseNum, exponent);
                        expression = expression.Replace(match.Value, powResult.ToString());
                    }
                }

                
                var dt = new System.Data.DataTable();
                var computeResult = dt.Compute(expression, "").ToString();

                
                if (double.TryParse(computeResult, out double numResult))
                {
                    
                    if (numResult == Math.Floor(numResult))
                    {
                        return ((int)numResult).ToString();
                    }
                    
                    return Math.Round(numResult, 2).ToString();
                }

                return computeResult;
            }
            catch (Exception ex)
            {
                SafeLog("Erreur de calcul : " + ex.Message);
                return "Erreur de calcul";
            }
        }

        private bool IsTerminalCommand(string text)
        {
            
            var terminalCommands = new[]
            {
                "dir", "cd", "mkdir", "rmdir", "copy", "move", "del", "type",
                "echo", "cls", "color", "date", "time", "ver", "vol", "attrib",
                "chkdsk", "format", "ipconfig", "ping", "tracert", "netstat",
                "tasklist", "taskkill", "shutdown", "start", "systeminfo"
            };

            
            return terminalCommands.Any(cmd => text.ToLower().StartsWith(cmd.ToLower()));
        }

        private void SetupWindowEvents()
        {
            
            Closing += MainWindow_Closing;
            Deactivated += MainWindow_Deactivated;

            
            _keyboardHook = new KeyboardHook();
            _keyboardHook.KeyPressed += (s, e) =>
            {
                if (e.Modifier == ModifierKeys.Control && e.Key == Key.Space)
                {
                    try
                    {
                        Dispatcher.Invoke(() =>
                        {
                            if (!IsVisible)
                            {
                                Show();
                                Activate();
                                SearchBox.Focus();
                            }
                            else
                            {
                                Hide();
                            }
                        });
                    }
                    catch {}
                }
            };
            _keyboardHook.RegisterHotKey(ModifierKeys.Control, Key.Space);

            
            _notifyIcon = new NotifyIcon();
            
            
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.ico");
            if (File.Exists(iconPath))
            {
                try
                {
                    _notifyIcon.Icon = new System.Drawing.Icon(iconPath);
                }
                catch (Exception iconEx)
                {
                    SafeLog($"Erreur lors du chargement de l'icône : {iconEx.Message}");
                    _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
                }
            }
            else
            {
                
                _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            }

            _notifyIcon.Visible = true;
            _notifyIcon.Text = "RayCast";

            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            var settingsItem = new System.Windows.Forms.ToolStripMenuItem("Paramètres");
            settingsItem.Click += (s, e) => 
            {
                Dispatcher.Invoke(() => 
                {
                    var settingsWindow = new SettingsWindow();
                    settingsWindow.Closed += (s, args) =>
                    {
                        LoadSettings();
                        ApplyTheme(currentSettings?.Theme ?? "Light");
                        RegisterUserHotkey();
                        if (!string.IsNullOrEmpty(SearchBox.Text))
                        {
                            _ = UpdateNormalSearch(SearchBox.Text);
                        }
                    };
                    settingsWindow.Show();
                });
            };
            contextMenu.Items.Add(settingsItem);

            var exitItem = new System.Windows.Forms.ToolStripMenuItem("Quitter");
            exitItem.Click += (s, e) => 
            {
                Dispatcher.Invoke(() => 
                {
                    _isExiting = true;
                    Close();
                });
            };
            contextMenu.Items.Add(exitItem);

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += (s, e) => 
            {
                Dispatcher.Invoke(() => 
                {
                    Show();
                    Activate();
                });
            };
        }

        private void ExitApplication(object? sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _keyboardHook?.Dispose();
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
        }

        private async Task GenerateIASuggestion()
        {
            if (string.IsNullOrEmpty(geminiApiKey) || string.IsNullOrEmpty(SearchBox.Text))
                return;

            var suggestion = await GetGeminiSuggestion(SearchBox.Text);
            if (!string.IsNullOrEmpty(suggestion))
            {
                
                var generateButton = searchResults.FirstOrDefault(r => r.ActionType == "generate_ia");
                if (generateButton != null)
                    searchResults.Remove(generateButton);

                
                searchResults.Add(new SearchResult
                {
                    Icon = "🤖",
                    Title = suggestion,
                    Description = "Suggestion Gemini",
                    ActionType = "web"
                });
            }
        }

        private void SearchBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    HideWindow();
                    e.Handled = true;
                    break;
                case Key.Enter:
                    if (ResultsList.SelectedItem is SearchResult selectedResult)
                    {
                        ExecuteAction(selectedResult);
                        HideWindow();
                    }
                    e.Handled = true;
                    break;
                case Key.Down:
                    if (ResultsList.Items.Count > 0)
                    {
                        ResultsList.SelectedIndex = ResultsList.SelectedIndex < 0 ? 0 : Math.Min(ResultsList.SelectedIndex + 1, ResultsList.Items.Count - 1);
                        ResultsList.Focus();
                        e.Handled = true;
                    }
                    break;
                case Key.Up:
                    if (ResultsList.Items.Count > 0)
                    {
                        ResultsList.SelectedIndex = ResultsList.SelectedIndex <= 0 ? ResultsList.Items.Count - 1 : ResultsList.SelectedIndex - 1;
                        ResultsList.Focus();
                        e.Handled = true;
                    }
                    break;
                case Key.I:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        _isIAActive = !_isIAActive;
                        if (!string.IsNullOrEmpty(SearchBox.Text))
                        {
                            _lastSearchText = SearchBox.Text;
                            _ = UpdateSearchResults(SearchBox.Text);
                        }
                        e.Handled = true;
                    }
                    break;
                case Key.V:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        if (System.Windows.Clipboard.ContainsText())
                        {
                            SearchBox.Text = System.Windows.Clipboard.GetText();
                            SearchBox.CaretIndex = SearchBox.Text.Length;
                            e.Handled = true;
                        }
                    }
                    break;
            }
        }

        private ImageSource? GetAppIcon(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    using var icon = System.Drawing.Icon.ExtractAssociatedIcon(path);
                    if (icon != null)
                    {
                        return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private async Task<ImageSource?> GetFavicon(string url)
        {
            try
            {
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "https://" + url;
                }

                var uri = new Uri(url);
                var faviconUrl = $"{uri.Scheme}://{uri.Host}/favicon.ico";

                using var client = new HttpClient();
                var response = await client.GetAsync(faviconUrl);
                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private void LaunchSelectedApp()
        {
            if (ResultsList.SelectedItem is SearchResult selectedResult && !string.IsNullOrEmpty(selectedResult.AppPath))
            {
                SafeLog($"Tentative de lancement de l'application : {selectedResult.AppPath}");
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = selectedResult.AppPath,
                        UseShellExecute = true,
                        Verb = "open"
                    };
                    
                    Process.Start(startInfo);
                    SafeLog($"Application lancée avec succès : {selectedResult.AppPath}");
                    HideWindow();
                }
                catch (Exception ex)
                {
                    SafeLog($"Erreur lors du lancement de l'application : {ex.Message}");
                }
            }
        }

        private void SearchBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.V:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        if (System.Windows.Clipboard.ContainsText())
                        {
                            SearchBox.SelectedText = System.Windows.Clipboard.GetText();
                            e.Handled = true;
                        }
                    }
                    break;
                case Key.Enter:
                    if (ResultsList.SelectedItem is SearchResult selectedResult)
                    {
                        ExecuteAction(selectedResult);
                        if (selectedResult.ActionType != "ia")
                        {
                            HideWindow();
                        }
                        e.Handled = true;
                    }
                    else if (ResultsList.Items.Count > 0)
                    {
                        ResultsList.SelectedIndex = 0;
                        if (ResultsList.SelectedItem is SearchResult firstResult)
                        {
                            ExecuteAction(firstResult);
                            if (firstResult.ActionType != "ia")
                            {
                                HideWindow();
                            }
                        }
                        e.Handled = true;
                    }
                    break;
            }

            
            if (!e.Handled)
            {
                MainWindow_KeyDown(sender, e);
            }
        }

        private void ActivateIA(string prompt)
        {
            SafeLog("Activation de l'IA avec le prompt : " + prompt);
            
            
            ResultsList.Visibility = Visibility.Collapsed;
            
            
            _isIAActive = true;
            SearchBox.Text = prompt;
            SearchBox.Visibility = Visibility.Visible;
            SearchBox.Focus();
            
            
            _ = UpdateIASearch(prompt);
        }

        private void SearchDelayTimer_Tick(object sender, EventArgs e)
        {
            _searchDelayTimer.Stop();
            _ = UpdateSearchResults(SearchBox.Text);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Closed += (s, args) =>
            {
                LoadSettings();
                ApplyTheme(currentSettings?.Theme ?? "Light");
                if (!string.IsNullOrEmpty(SearchBox.Text))
                {
                    _ = UpdateNormalSearch(SearchBox.Text);
                }
            };
            settingsWindow.Show();
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists("config.json"))
                {
                    string json = File.ReadAllText("config.json");
                    currentSettings = JsonConvert.DeserializeObject<Settings>(json);
                    if (currentSettings != null)
                    {
                        ApplyTheme(currentSettings.Theme);
                    }
                }
                else
                {
                    currentSettings = new Settings();
                    ApplyTheme(currentSettings.Theme);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors du chargement des paramètres : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyTheme(string theme)
        {
            if (theme?.ToLower() == "dark" || theme?.ToLower() == "sombre")
            {
                System.Windows.Application.Current.Resources["BackgroundColor"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 34, 34));
                System.Windows.Application.Current.Resources["ForegroundColor"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 240, 240));
                System.Windows.Application.Current.Resources["BorderColor"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(60, 60, 60));
                System.Windows.Application.Current.Resources["HoverColor"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(50, 50, 50));
                System.Windows.Application.Current.Resources["SelectionColor"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(80, 80, 80));
                System.Windows.Application.Current.Resources["SubtitleColor"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(180, 180, 180));
            }
            else // clair par défaut
            {
                System.Windows.Application.Current.Resources["BackgroundColor"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                System.Windows.Application.Current.Resources["ForegroundColor"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                System.Windows.Application.Current.Resources["BorderColor"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(224, 224, 224));
                System.Windows.Application.Current.Resources["HoverColor"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 245, 245));
                System.Windows.Application.Current.Resources["SelectionColor"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(224, 224, 224));
                System.Windows.Application.Current.Resources["SubtitleColor"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(102, 102, 102));
            }
            UpdateTheme();
        }

        public void UpdateTheme()
        {
            
            SearchBox.Background = (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["BackgroundColor"];
            SearchBox.Foreground = (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["ForegroundColor"];
            SearchBox.BorderBrush = (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["BorderColor"];

            ResultsList.Background = (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["BackgroundColor"];
            ResultsList.Foreground = (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["ForegroundColor"];
            ResultsList.BorderBrush = (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["BorderColor"];

            
            var style = new Style(typeof(ListBoxItem));
            style.Setters.Add(new Setter(System.Windows.Controls.Control.BackgroundProperty, (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["BackgroundColor"]));
            style.Setters.Add(new Setter(System.Windows.Controls.Control.ForegroundProperty, (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["ForegroundColor"]));
            style.Triggers.Add(new Trigger { Property = ListBoxItem.IsSelectedProperty, Value = true, Setters = { new Setter(System.Windows.Controls.Control.BackgroundProperty, (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["SelectionColor"]) } });
            style.Triggers.Add(new Trigger { Property = ListBoxItem.IsMouseOverProperty, Value = true, Setters = { new Setter(System.Windows.Controls.Control.BackgroundProperty, (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["HoverColor"]) } });
            ResultsList.ItemContainerStyle = style;
        }

        private void InitializeSystemTray()
        {
            try
            {
                if (_notifyIcon != null)
                {
                    _notifyIcon.Dispose();
                    _notifyIcon = null;
                }

                _notifyIcon = new NotifyIcon();
                
                
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.ico");
                if (File.Exists(iconPath))
                {
                    try
                    {
                        _notifyIcon.Icon = new System.Drawing.Icon(iconPath);
                    }
                    catch (Exception iconEx)
                    {
                        SafeLog($"Erreur lors du chargement de l'icône : {iconEx.Message}");
                        _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
                    }
                }
                else
                {
                    
                    _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
                }

                _notifyIcon.Visible = true;
                _notifyIcon.Text = "RayCast";

                var contextMenu = new System.Windows.Forms.ContextMenuStrip();
                var settingsItem = new System.Windows.Forms.ToolStripMenuItem("Paramètres");
                settingsItem.Click += (s, e) => 
                {
                    Dispatcher.Invoke(() => 
                    {
                        var settingsWindow = new SettingsWindow();
                        settingsWindow.Closed += (s, args) =>
                        {
                            LoadSettings();
                            ApplyTheme(currentSettings?.Theme ?? "Light");
                            RegisterUserHotkey();
                            if (!string.IsNullOrEmpty(SearchBox.Text))
                            {
                                _ = UpdateNormalSearch(SearchBox.Text);
                            }
                        };
                        settingsWindow.Show();
                    });
                };
                contextMenu.Items.Add(settingsItem);

                var exitItem = new System.Windows.Forms.ToolStripMenuItem("Quitter");
                exitItem.Click += (s, e) => 
                {
                    Dispatcher.Invoke(() => 
                    {
                        _isExiting = true;
                        Close();
                    });
                };
                contextMenu.Items.Add(exitItem);

                _notifyIcon.ContextMenuStrip = contextMenu;
                _notifyIcon.DoubleClick += (s, e) => 
                {
                    Dispatcher.Invoke(() => 
                    {
                        Show();
                        Activate();
                    });
                };
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de l'initialisation de la systray : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_isExiting)
            {
                e.Cancel = true;
                Hide();
                return;
            }

            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }

            base.OnClosing(e);
        }

        private void ResultsList_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (ResultsList.SelectedItem is SearchResult selectedResult)
                    {
                        ExecuteAction(selectedResult);
                        if (selectedResult.ActionType != "ia")
                        {
                            HideWindow();
                        }
                        e.Handled = true;
                    }
                    break;
                case Key.Down:
                    if (ResultsList.Items.Count > 0)
                    {
                        ResultsList.SelectedIndex = ResultsList.SelectedIndex < 0 ? 0 : Math.Min(ResultsList.SelectedIndex + 1, ResultsList.Items.Count - 1);
                        ResultsList.ScrollIntoView(ResultsList.SelectedItem);
                        e.Handled = true;
                    }
                    break;
                case Key.Up:
                    if (ResultsList.Items.Count > 0)
                    {
                        ResultsList.SelectedIndex = ResultsList.SelectedIndex <= 0 ? ResultsList.Items.Count - 1 : ResultsList.SelectedIndex - 1;
                        ResultsList.ScrollIntoView(ResultsList.SelectedItem);
                        e.Handled = true;
                    }
                    break;
                case Key.Escape:
                    HideWindow();
                    e.Handled = true;
                    break;
            }
        }

        private void ChangeHotkeyButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Appuyez sur la nouvelle combinaison de touches pour le raccourci.", "Changer le raccourci", MessageBoxButton.OK, MessageBoxImage.Information);
            
            var hook = new GlobalKeyboardHook();
            hook.KeyDown += (s, args) =>
            {
                if (args.Key == Key.Escape)
                {
                    System.Windows.MessageBox.Show("Changement de raccourci annulé.", "Annulé", MessageBoxButton.OK, MessageBoxImage.Information);
                    hook.Dispose();
                    return;
                }

                var modifiers = Keyboard.Modifiers;
                var key = args.Key;
                var newHotkey = new HotKey(0, modifiers, key);
                _hotKey?.Dispose();
                _hotKey = newHotkey;
                _hotKey.Pressed += HotKey_Pressed;
                System.Windows.MessageBox.Show($"Nouveau raccourci enregistré : {modifiers} + {key}", "Raccourci changé", MessageBoxButton.OK, MessageBoxImage.Information);
                hook.Dispose();
            };
        }

        private class GlobalKeyboardHook : IDisposable
        {
            private const int WH_KEYBOARD_LL = 13;
            private const int WM_KEYDOWN = 0x0100;
            private LowLevelKeyboardProc _proc;
            private IntPtr _hookID = IntPtr.Zero;

            public event EventHandler<System.Windows.Input.KeyEventArgs>? KeyDown;

            public GlobalKeyboardHook()
            {
                _proc = HookCallback;
                _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
            }

            private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    var key = KeyInterop.KeyFromVirtualKey(vkCode);
                    KeyDown?.Invoke(this, new System.Windows.Input.KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, key));
                }
                return CallNextHookEx(_hookID, nCode, wParam, lParam);
            }

            public void Dispose()
            {
                UnhookWindowsHookEx(_hookID);
            }

            private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr GetModuleHandle(string lpModuleName);
        }

        private void IAResponseBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void SafeLog(string message)
        {
            try
            {
                using (var stream = new System.IO.FileStream("raycast.log", System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite))
                using (var writer = new System.IO.StreamWriter(stream))
                {
                    writer.WriteLine($"[{System.DateTime.Now}] {message}");
                }
            }
            catch { }
        }

        /// <summary>
        /// Recherche tous les exécutables (.exe) sur tous les disques locaux (hors dossiers système)
        /// </summary>
        private void SearchAllExecutablesOnAllDrives()
        {
            try
            {
                var exeList = new List<ExeCacheEntry>();
                var systemFolders = new[]
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    Environment.GetFolderPath(Environment.SpecialFolder.System),
                    Environment.GetFolderPath(Environment.SpecialFolder.SystemX86)
                };
                var drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed && d.IsReady);
                foreach (var drive in drives)
                {
                    try
                    {
                        var root = drive.RootDirectory.FullName;
                        var exeFiles = Directory.EnumerateFiles(root, "*.exe", SearchOption.AllDirectories)
                            .Where(f => !systemFolders.Any(sys => f.StartsWith(sys, StringComparison.OrdinalIgnoreCase)));
                        foreach (var exeFile in exeFiles)
                        {
                            try
                            {
                                var fileInfo = FileVersionInfo.GetVersionInfo(exeFile);
                                var displayName = !string.IsNullOrEmpty(fileInfo.FileDescription)
                                    ? fileInfo.FileDescription
                                    : Path.GetFileNameWithoutExtension(exeFile);
                                exeList.Add(new ExeCacheEntry
                                {
                                    Path = exeFile,
                                    DisplayName = displayName,
                                    LastModified = File.GetLastWriteTime(exeFile)
                                });
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
                // Ajout au cache
                var json = JsonConvert.SerializeObject(exeList);
                File.WriteAllText(CACHE_FILE, json);
                SafeLog($"Recherche exhaustive terminée : {exeList.Count} exécutables trouvés.");
            }
            catch (Exception ex)
            {
                SafeLog("Erreur lors de la recherche exhaustive : " + ex.Message);
            }
        }

        /// <summary>
        /// Recherche les .exe via l'indexation Windows Search (rapide si activée)
        /// </summary>
        private void SearchExecutablesWithWindowsSearch()
        {
            try
            {
                var exeList = new List<ExeCacheEntry>();
                var query = "SELECT System.ItemPathDisplay FROM SystemIndex WHERE System.FileExtension = '.exe'";
                using (var searcher = new ManagementObjectSearcher("root\\Search", $"SELECT System.ItemPathDisplay FROM SystemIndex WHERE System.FileExtension = '.exe'"))
                {
                    foreach (ManagementObject result in searcher.Get())
                    {
                        var path = result["System.ItemPathDisplay"]?.ToString();
                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        {
                            try
                            {
                                var fileInfo = FileVersionInfo.GetVersionInfo(path);
                                var displayName = !string.IsNullOrEmpty(fileInfo.FileDescription)
                                    ? fileInfo.FileDescription
                                    : Path.GetFileNameWithoutExtension(path);
                                exeList.Add(new ExeCacheEntry
                                {
                                    Path = path,
                                    DisplayName = displayName,
                                    LastModified = File.GetLastWriteTime(path)
                                });
                            }
                            catch { }
                        }
                    }
                }
                var json = JsonConvert.SerializeObject(exeList);
                File.WriteAllText(CACHE_FILE, json);
                SafeLog($"Recherche Windows Search terminée : {exeList.Count} exécutables trouvés.");
            }
            catch (Exception ex)
            {
                SafeLog("Erreur lors de la recherche Windows Search : " + ex.Message);
            }
        }

        /// <summary>
        /// Recherche les raccourcis .lnk dans tous les dossiers utilisateur (Bureau, Documents, etc.)
        /// </summary>
        private void SearchShortcutsInUserFolders()
        {
            try
            {
                var exeList = new List<ExeCacheEntry>();
                var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var userFolders = new[]
                {
                    Path.Combine(userProfile, "Desktop"),
                    Path.Combine(userProfile, "Documents"),
                    Path.Combine(userProfile, "Downloads"),
                    Path.Combine(userProfile, "Bureau"),
                    Path.Combine(userProfile, "Mes documents")
                };
                foreach (var folder in userFolders)
                {
                    if (Directory.Exists(folder))
                    {
                        var shortcuts = Directory.GetFiles(folder, "*.lnk", SearchOption.AllDirectories);
                        foreach (var shortcut in shortcuts)
                        {
                            var targetPath = GetShortcutTarget(shortcut);
                            if (!string.IsNullOrEmpty(targetPath) && File.Exists(targetPath) && targetPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                            {
                                var fileInfo = FileVersionInfo.GetVersionInfo(targetPath);
                                var displayName = !string.IsNullOrEmpty(fileInfo.FileDescription)
                                    ? fileInfo.FileDescription
                                    : Path.GetFileNameWithoutExtension(targetPath);
                                exeList.Add(new ExeCacheEntry
                                {
                                    Path = targetPath,
                                    DisplayName = displayName,
                                    LastModified = File.GetLastWriteTime(targetPath)
                                });
                            }
                        }
                    }
                }
                var json = JsonConvert.SerializeObject(exeList);
                File.WriteAllText(CACHE_FILE, json);
                SafeLog($"Recherche des raccourcis utilisateur terminée : {exeList.Count} exécutables trouvés.");
            }
            catch (Exception ex)
            {
                SafeLog("Erreur lors de la recherche des raccourcis utilisateur : " + ex.Message);
            }
        }

        /// <summary>
        /// Permet à l'utilisateur d'ajouter des dossiers personnalisés à scanner
        /// </summary>
        public void AddCustomFolderToScan(string folderPath)
        {
            try
            {
                if (Directory.Exists(folderPath))
                {
                    var exeList = new List<ExeCacheEntry>();
                    var exeFiles = Directory.GetFiles(folderPath, "*.exe", SearchOption.AllDirectories);
                    foreach (var exeFile in exeFiles)
                    {
                        var fileInfo = FileVersionInfo.GetVersionInfo(exeFile);
                        var displayName = !string.IsNullOrEmpty(fileInfo.FileDescription)
                            ? fileInfo.FileDescription
                            : Path.GetFileNameWithoutExtension(exeFile);
                        exeList.Add(new ExeCacheEntry
                        {
                            Path = exeFile,
                            DisplayName = displayName,
                            LastModified = File.GetLastWriteTime(exeFile)
                        });
                    }
                    var json = JsonConvert.SerializeObject(exeList);
                    File.WriteAllText(CACHE_FILE, json);
                    SafeLog($"Scan du dossier personnalisé terminé : {exeList.Count} exécutables trouvés.");
                }
            }
            catch (Exception ex)
            {
                SafeLog("Erreur lors du scan du dossier personnalisé : " + ex.Message);
            }
        }

        private void RegisterUserHotkey()
        {
            if (currentSettings == null || string.IsNullOrEmpty(currentSettings.Hotkey) || !currentSettings.IsHotkeyEnabled)
                return;
            try
            {
                // Parser le texte du raccourci (ex: Ctrl+Alt+Space)
                var parts = currentSettings.Hotkey.Split('+');
                ModifierKeys modifiers = ModifierKeys.None;
                Key key = Key.None;
                foreach (var part in parts)
                {
                    var p = part.Trim();
                    if (p.Equals("Ctrl", StringComparison.OrdinalIgnoreCase)) modifiers |= ModifierKeys.Control;
                    else if (p.Equals("Alt", StringComparison.OrdinalIgnoreCase)) modifiers |= ModifierKeys.Alt;
                    else if (p.Equals("Shift", StringComparison.OrdinalIgnoreCase)) modifiers |= ModifierKeys.Shift;
                    else if (p.Equals("Win", StringComparison.OrdinalIgnoreCase)) modifiers |= ModifierKeys.Windows;
                    else if (!string.IsNullOrWhiteSpace(p))
                    {
                        if (Enum.TryParse<Key>(p, true, out var parsedKey))
                            key = parsedKey;
                    }
                }
                if (key != Key.None)
                {
                    _hotKey?.Dispose();
                    var handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                    _hotKey = new HotKey(handle, modifiers, key);
                    _hotKey.Pressed += HotKey_Pressed;
                    SafeLog($"Raccourci utilisateur enregistré : {currentSettings.Hotkey}");
                }
            }
            catch (Exception ex)
            {
                SafeLog($"Erreur lors de l'enregistrement du raccourci utilisateur : {ex.Message}");
            }
        }
    }

    public class SearchResult
    {
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string? AppPath { get; set; }
        public System.Windows.Media.ImageSource? AppIcon { get; set; }
        public bool IsLoading { get; set; }
        public string? WebUrl { get; set; }
        public Action? Action { get; set; }
    }

    public class AppInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }
}