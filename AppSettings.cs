 using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Bin11
{
    
    public class AppSettings
    {
        // Спрашивать подтверждение перед очисткой корзины.
        public bool ConfirmBeforeEmpty { get; set; } = true;

        // Запускать вместе с Windows.
        public bool RunAtStartup { get; set; } = false;

        // Показывать уведомление (toast/balloon), когда корзина почти заполнена.
        public bool NotifyWhenFull { get; set; } = true;

        private static string SettingsPath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Bin11", "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                    if (loaded != null) return loaded;
                }
            }
            catch
            {
                // Если файл повреждён — используем настройки по умолчанию.
            }
            return new AppSettings();
        }

        public void Save()
        {
            var dir = Path.GetDirectoryName(SettingsPath)!;
            Directory.CreateDirectory(dir);
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);

            ApplyStartupSetting();
        }

        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string RunValueName = "Bin11";

        private void ApplyStartupSetting()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
            if (key == null) return;

            if (RunAtStartup)
            {
                string exePath = Environment.ProcessPath ?? Application.ExecutablePath;
                key.SetValue(RunValueName, $"\"{exePath}\"");
            }
            else
            {
                if (key.GetValue(RunValueName) != null)
                    key.DeleteValue(RunValueName, throwOnMissingValue: false);
            }
        }
    }
}
