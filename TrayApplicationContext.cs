using System;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Bin11
{
    public class TrayApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly RecycleBinMonitor _monitor;
        private readonly AppSettings _settings;
        private readonly SynchronizationContext _uiContext;

        private bool _notifiedFullAlready = false;

        public TrayApplicationContext()
        {
            Logger.Log("TrayApplicationContext: старт конструктора.");

            _settings = AppSettings.Load();
            Logger.Log("TrayApplicationContext: настройки загружены.");

            
            if (SynchronizationContext.Current is not System.Windows.Forms.WindowsFormsSynchronizationContext)
            {
                SynchronizationContext.SetSynchronizationContext(
                    new System.Windows.Forms.WindowsFormsSynchronizationContext());
            }
            _uiContext = SynchronizationContext.Current!;
            Logger.Log("TrayApplicationContext: синхронизационный контекст установлен.");

            var initialIcon = IconRenderer.CreateIcon(0.0);
            Logger.Log("TrayApplicationContext: начальная иконка отрисована.");

            _notifyIcon = new NotifyIcon
            {
                Icon = initialIcon,
                Visible = true,
                Text = "Bin11 — корзина пуста"
            };
            Logger.Log("TrayApplicationContext: NotifyIcon создан и сделан видимым.");

            _notifyIcon.ContextMenuStrip = BuildContextMenu();
            Logger.Log("TrayApplicationContext: контекстное меню построено.");

            // Двойной левый клик -> очистка корзины.
            _notifyIcon.MouseDoubleClick += (_, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    EmptyRecycleBin();
            };

            
            SystemEvents.UserPreferenceChanged += OnSystemPreferenceChanged;
            Logger.Log("TrayApplicationContext: подписка на SystemEvents оформлена.");

            _monitor = new RecycleBinMonitor();
            _monitor.UsageChanged += OnUsageChanged;
            Logger.Log("TrayApplicationContext: RecycleBinMonitor создан, watcher'ы настроены.");

            // Первичная отрисовка при запуске.
            _monitor.RaiseUsageChanged();
            Logger.Log("TrayApplicationContext: конструктор завершён успешно.");
        }

        private ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();

            menu.Items.Add("Открыть корзину", null, (_, _) => OpenRecycleBinFolder());
            menu.Items.Add("Очистить корзину", null, (_, _) => EmptyRecycleBin());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Настройки...", null, (_, _) => OpenSettings());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Выход", null, (_, _) => ExitApplication());

            return menu;
        }

        private void OpenRecycleBinFolder()
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = "shell:RecycleBinFolder",
                    UseShellExecute = true
                });
                Logger.Log("OpenRecycleBinFolder: окно корзины открыто.");
            }
            catch (Exception ex)
            {
                Logger.LogException("OpenRecycleBinFolder", ex);
            }
        }

        private void OnUsageChanged(object? sender, RecycleBinUsageEventArgs e)
        {
            
            _uiContext.Post(_ => UpdateIconAndTooltip(e), null);
        }

        private void UpdateIconAndTooltip(RecycleBinUsageEventArgs e)
        {
            try
            {
                long thresholdBytes = RecycleBinCapacity.GetTotalCapacityBytes();
                double percent = thresholdBytes > 0 ? (double)e.SizeBytes / thresholdBytes : 0;
                percent = Math.Clamp(percent, 0.0, 1.0);

                var oldIcon = _notifyIcon.Icon;
                _notifyIcon.Icon = IconRenderer.CreateIcon(percent);
                oldIcon?.Dispose();

                double sizeMb = e.SizeBytes / 1024.0 / 1024.0;
                string sizeText = sizeMb >= 1024
                    ? $"{sizeMb / 1024.0:F2} ГБ"
                    : $"{sizeMb:F1} МБ";

                double thresholdGb = thresholdBytes / 1024.0 / 1024.0 / 1024.0;

                // У NotifyIcon.Text есть ограничение в 127 символов.
                _notifyIcon.Text = $"Корзина: {sizeText} ({e.ItemCount} объектов), {percent:P0} от лимита";

                if (_settings.NotifyWhenFull && percent >= 1.0 && !_notifiedFullAlready)
                {
                    _notifyIcon.ShowBalloonTip(4000, "Корзина почти заполнена",
                        $"Занято {sizeText}, что превышает лимит корзины на этом ПК (~{thresholdGb:F1} ГБ).",
                        ToolTipIcon.Warning);
                    _notifiedFullAlready = true;
                }
                else if (percent < 0.9)
                {
                    _notifiedFullAlready = false;
                }

                Logger.Log($"UpdateIconAndTooltip: успешно, size={e.SizeBytes}, threshold={thresholdBytes}, percent={percent:P0}.");
            }
            catch (Exception ex)
            {
                Logger.LogException("UpdateIconAndTooltip", ex);
            }
        }

        private void OnSystemPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General ||
                e.Category == UserPreferenceCategory.Color)
            {
                // Тема или акцентный цвет могли поменяться — форсируем перерисовку.
                _monitor.RaiseUsageChanged();
            }
        }

        private void EmptyRecycleBin()
        {
            if (_settings.ConfirmBeforeEmpty)
            {
                var result = MessageBox.Show(
                    "Очистить корзину на всех дисках? Это действие нельзя отменить.",
                    "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                    return;
            }

            NativeMethods.EmptyAll(askConfirmation: false);
            _monitor.RaiseUsageChanged();
        }

        private void OpenSettings()
        {
            using var form = new SettingsForm(_settings);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _notifiedFullAlready = false;
                _monitor.RaiseUsageChanged();
            }
        }

        private void ExitApplication()
        {
            _notifyIcon.Visible = false;
            SystemEvents.UserPreferenceChanged -= OnSystemPreferenceChanged;
            _monitor.Dispose();
            _notifyIcon.Dispose();
            Application.Exit();
        }
    }
}
