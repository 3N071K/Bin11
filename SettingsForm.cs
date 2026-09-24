using System;
using System.Drawing;
using System.Windows.Forms;

namespace Bin11
{
    public class SettingsForm : Form
    {
        private readonly AppSettings _settings;
        private readonly CheckBox _confirmCheckbox;
        private readonly CheckBox _startupCheckbox;
        private readonly CheckBox _notifyCheckbox;

        public SettingsForm(AppSettings settings)
        {
            _settings = settings;

            Text = "Bin11 — настройки";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(340, 170);
            Font = new Font("Segoe UI", 9f);

            var infoLabel = new Label
            {
                Text = "Порог заполнения (100%) берётся из настроек\nкорзины Windows для каждого диска.",
                Location = new Point(15, 15),
                AutoSize = true,
                ForeColor = SystemColors.GrayText
            };

            _confirmCheckbox = new CheckBox
            {
                Text = "Спрашивать подтверждение перед очисткой",
                Location = new Point(15, 55),
                AutoSize = true,
                Checked = _settings.ConfirmBeforeEmpty
            };

            _startupCheckbox = new CheckBox
            {
                Text = "Запускать вместе с Windows",
                Location = new Point(15, 85),
                AutoSize = true,
                Checked = _settings.RunAtStartup
            };

            _notifyCheckbox = new CheckBox
            {
                Text = "Уведомлять, когда корзина почти заполнена",
                Location = new Point(15, 115),
                AutoSize = true,
                Checked = _settings.NotifyWhenFull
            };

            var saveButton = new Button
            {
                Text = "Сохранить",
                Location = new Point(140, 135),
                Width = 90,
                DialogResult = DialogResult.OK
            };
            saveButton.Click += (_, _) => SaveAndClose();

            var cancelButton = new Button
            {
                Text = "Отмена",
                Location = new Point(235, 135),
                Width = 90,
                DialogResult = DialogResult.Cancel
            };

            Controls.AddRange(new Control[]
            {
                infoLabel,
                _confirmCheckbox, _startupCheckbox, _notifyCheckbox,
                saveButton, cancelButton
            });

            AcceptButton = saveButton;
            CancelButton = cancelButton;
        }

        private void SaveAndClose()
        {
            _settings.ConfirmBeforeEmpty = _confirmCheckbox.Checked;
            _settings.RunAtStartup = _startupCheckbox.Checked;
            _settings.NotifyWhenFull = _notifyCheckbox.Checked;
            _settings.Save();
        }
    }
}
