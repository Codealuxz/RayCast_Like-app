using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using Newtonsoft.Json;

namespace RayCast
{
    public class Scanner
    {
        public class ExeInfo
        {
            public string Path { get; set; }
            public string DisplayName { get; set; }
            public DateTime LastModified { get; set; }
        }

        public List<ExeInfo> Results { get; private set; } = new List<ExeInfo>();
        public string CacheFile { get; set; } = "exe_cache.json";
        public List<string> CustomFolders { get; set; } = new List<string>();

        private readonly string[] systemFolders = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            Environment.GetFolderPath(Environment.SpecialFolder.SystemX86)
        };

        public void ScanAllDrives()
        {
            Results.Clear();
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
                            Results.Add(new ExeInfo
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
        }

        public void ScanWithWindowsSearch()
        {
            try
            {
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
                                Results.Add(new ExeInfo
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
            }
            catch { }
        }

        public void ScanUserShortcuts()
        {
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
                            Results.Add(new ExeInfo
                            {
                                Path = targetPath,
                                DisplayName = displayName,
                                LastModified = File.GetLastWriteTime(targetPath)
                            });
                        }
                    }
                }
            }
        }

        public void ScanCustomFolders()
        {
            foreach (var folderPath in CustomFolders)
            {
                if (Directory.Exists(folderPath))
                {
                    var exeFiles = Directory.GetFiles(folderPath, "*.exe", SearchOption.AllDirectories);
                    foreach (var exeFile in exeFiles)
                    {
                        var fileInfo = FileVersionInfo.GetVersionInfo(exeFile);
                        var displayName = !string.IsNullOrEmpty(fileInfo.FileDescription)
                            ? fileInfo.FileDescription
                            : Path.GetFileNameWithoutExtension(exeFile);
                        Results.Add(new ExeInfo
                        {
                            Path = exeFile,
                            DisplayName = displayName,
                            LastModified = File.GetLastWriteTime(exeFile)
                        });
                    }
                }
            }
        }

        public void FilterAndDeduplicate()
        {
            Results = Results
                .GroupBy(e => e.Path.ToLower())
                .Select(g => g.First())
                .OrderBy(e => e.DisplayName)
                .ToList();
        }

        public void SaveCache()
        {
            var json = JsonConvert.SerializeObject(Results);
            File.WriteAllText(CacheFile, json);
        }

        public void LoadCache()
        {
            if (File.Exists(CacheFile))
            {
                var json = File.ReadAllText(CacheFile);
                Results = JsonConvert.DeserializeObject<List<ExeInfo>>(json) ?? new List<ExeInfo>();
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
    }
} 