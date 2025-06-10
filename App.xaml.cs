using System;
using System.Windows;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using System.IO;

namespace RayCast
{
    public partial class App : System.Windows.Application
    {
        private static Mutex? _mutex = null;
        private const string AppName = "RayCast";

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                string startupLogPath = Path.Combine(Path.GetTempPath(), "raycast_startup.log");
                System.IO.File.AppendAllText(startupLogPath, $"[{DateTime.Now}] Démarrage de l'application.\n");
                
                _mutex = new Mutex(true, "RayCastMutex", out bool createdNew);
                System.IO.File.AppendAllText(startupLogPath, $"[{DateTime.Now}] Mutex créé : {createdNew}\n");

                if (!createdNew)
                {
                    System.IO.File.AppendAllText(startupLogPath, $"[{DateTime.Now}] Une instance est déjà en cours d'exécution.\n");
                    
                    var existingWindow = Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    if (existingWindow != null)
                    {
                        System.IO.File.AppendAllText(startupLogPath, $"[{DateTime.Now}] Fenêtre existante trouvée, activation.\n");
                        existingWindow.Show();
                        existingWindow.Activate();
                        existingWindow.WindowState = WindowState.Normal;
                        existingWindow.Focus();
                    }
                    else
                    {
                        System.IO.File.AppendAllText(startupLogPath, $"[{DateTime.Now}] Fenêtre existante non trouvée, arrêt de l'application.\n");
                    }
                    Shutdown();
                    return;
                }

                System.IO.File.AppendAllText(startupLogPath, $"[{DateTime.Now}] Nettoyage des icônes de la zone de notification.\n");
                
                CleanupNotificationIcons();

                base.OnStartup(e);
                System.IO.File.AppendAllText(startupLogPath, $"[{DateTime.Now}] Création de la fenêtre principale.\n");
                var mainWindow = new MainWindow();
                mainWindow.Show();
                System.IO.File.AppendAllText(startupLogPath, $"[{DateTime.Now}] Fenêtre principale affichée.\n");
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(Path.Combine(Path.GetTempPath(), "raycast_startup.log"), $"[{DateTime.Now}] Erreur au démarrage : {ex.Message}\n");
                System.Windows.MessageBox.Show($"Erreur au démarrage : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private void CleanupNotificationIcons()
        {
            try
            {
                
                var processes = Process.GetProcessesByName("RayCast");
                foreach (var process in processes)
                {
                    if (process.Id != Process.GetCurrentProcess().Id)
                    {
                        process.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors du nettoyage des icônes : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            base.OnExit(e);
        }
    }
} 