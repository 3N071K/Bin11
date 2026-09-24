using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Bin11
{
    public class RecycleBinUsageEventArgs : EventArgs
    {
        public long SizeBytes { get; init; }
        public long ItemCount { get; init; }
    }

    
    public class RecycleBinMonitor : IDisposable
    {
        public event EventHandler<RecycleBinUsageEventArgs>? UsageChanged;

        private readonly List<FileSystemWatcher> _watchers = new();
        private readonly System.Threading.Timer _debounceTimer;
        private readonly System.Threading.Timer _fallbackTimer;
        private readonly object _lock = new();

        public RecycleBinMonitor()
        {
            
            _debounceTimer = new System.Threading.Timer(_ => RaiseUsageChanged(), null, Timeout.Infinite, Timeout.Infinite);

            
            _fallbackTimer = new System.Threading.Timer(_ => RaiseUsageChanged(), null, 60_000, 60_000);

            SetupWatchers();
        }

        private void SetupWatchers()
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType != DriveType.Fixed || !drive.IsReady)
                    continue;

                string recycleBinPath = Path.Combine(drive.RootDirectory.FullName, "$Recycle.Bin");
                if (!Directory.Exists(recycleBinPath))
                    continue;

                try
                {
                    var watcher = new FileSystemWatcher(recycleBinPath)
                    {
                        IncludeSubdirectories = true,
                        NotifyFilter = NotifyFilters.FileName
                                     | NotifyFilters.DirectoryName
                                     | NotifyFilters.Size,
                        EnableRaisingEvents = true
                    };

                    watcher.Created += OnRecycleBinChanged;
                    watcher.Deleted += OnRecycleBinChanged;
                    watcher.Renamed += OnRecycleBinChanged;
                    watcher.Error += (_, _) => { /* диск мог отключиться — игнорируем */ };

                    _watchers.Add(watcher);
                }
                catch
                {
                    
                }
            }
        }

        private void OnRecycleBinChanged(object sender, FileSystemEventArgs e)
        {
            lock (_lock)
            {
                
                _debounceTimer.Change(400, Timeout.Infinite);
            }
        }

        
        public void RaiseUsageChanged()
        {
            try
            {
                Logger.Log("RaiseUsageChanged: вызываем SHQueryRecycleBin...");
                var (size, items) = NativeMethods.GetTotalRecycleBinUsage();
                Logger.Log($"RaiseUsageChanged: получено size={size}, items={items}.");
                UsageChanged?.Invoke(this, new RecycleBinUsageEventArgs { SizeBytes = size, ItemCount = items });
                Logger.Log("RaiseUsageChanged: подписчики уведомлены.");
            }
            catch (Exception ex)
            {
                Logger.LogException("RaiseUsageChanged", ex);
            }
        }

        public void Dispose()
        {
            foreach (var w in _watchers)
            {
                w.EnableRaisingEvents = false;
                w.Dispose();
            }
            _debounceTimer.Dispose();
            _fallbackTimer.Dispose();
        }
    }
}
