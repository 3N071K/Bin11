using System;
using System.IO;

namespace Bin11
{
    
    internal static class Logger
    {
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Bin11", "log.txt");

        public static void Log(string message)
        {
            try
            {
                var dir = Path.GetDirectoryName(LogPath)!;
                Directory.CreateDirectory(dir);
                File.AppendAllText(LogPath, $"{DateTime.Now:HH:mm:ss.fff}  {message}{Environment.NewLine}");
            }
            catch
            {
                
            }
        }

        public static void LogException(string context, Exception ex)
        {
            Log($"### ОШИБКА в [{context}] ###{Environment.NewLine}{ex}");
        }
    }
}
