using System;
using System.Windows.Forms;

namespace Bin11
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Logger.Log("=== Запуск Bin11 ===");
            try
            {
                using var mutex = new System.Threading.Mutex(true, "Bin11_SingleInstance", out bool isNew);
                Logger.Log($"Mutex создан, isNew={isNew}");

                if (!isNew)
                {
                    Logger.Log("Уже запущен другой экземпляр — показываем сообщение и выходим.");
                    MessageBox.Show("Bin11 уже запущен.", "Bin11",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Application.ThreadException += (_, e) =>
                {
                    Logger.LogException("UI-поток (ThreadException)", e.Exception);
                    MessageBox.Show(e.Exception.ToString(), "Bin11 — необработанная ошибка (UI-поток)",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                };

                AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                {
                    if (e.ExceptionObject is Exception ex)
                        Logger.LogException("Фоновый поток (UnhandledException)", ex);
                    else
                        Logger.Log($"Фоновый поток (UnhandledException, не Exception): {e.ExceptionObject}");

                    MessageBox.Show(e.ExceptionObject?.ToString() ?? "Unknown error",
                        "Bin11 — необработанная ошибка (фоновый поток)",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                };

                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Logger.Log("Обработчики исключений установлены.");

                ApplicationConfiguration.Initialize();
                Logger.Log("ApplicationConfiguration.Initialize() выполнен успешно.");

                Logger.Log("Создаём TrayApplicationContext...");
                var context = new TrayApplicationContext();
                Logger.Log("TrayApplicationContext создан успешно. Запускаем Application.Run...");

                Application.Run(context);
                Logger.Log("Application.Run завершился (нормальный выход из приложения).");
            }
            catch (Exception ex)
            {
                Logger.LogException("Main (верхний уровень)", ex);
                MessageBox.Show($"Критическая ошибка при запуске Bin11:\n\n{ex}",
                    "Bin11 — ошибка запуска", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
