using System.Drawing;
using System.Windows.Forms;

namespace BatteryLife
{
    public sealed class NotifyIconHandler
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly int _timeOut;

        private string _backupBaloonTipText = string.Empty;

        public bool Visible
        {
            set => _notifyIcon.Visible = value;
        }

        public NotifyIconHandler(ref NotifyIcon notifyIcon, int timeOut, Icon icon = null)
        {
            _notifyIcon = notifyIcon;
            _notifyIcon.Icon = icon ?? SystemIcons.Application;
            _timeOut = timeOut;
        }

        public void ShowLastMessage() => _notifyIcon.ShowBalloonTip(_timeOut);

        public void ShowMessage(string baloonTipText, ToolTipIcon icon = ToolTipIcon.None)
        {
            SetNewMessage(baloonTipText, icon);

            ShowLastMessage();

            BackupText();
        }

        public void SetNewMessage(string baloonTipText, ToolTipIcon icon = ToolTipIcon.None)
        {
            _notifyIcon.BalloonTipIcon = icon;
            _notifyIcon.BalloonTipText = baloonTipText;
            _backupBaloonTipText = baloonTipText;
            Visible = true;
        }

        public void ResetProperties()
        {
            _notifyIcon.BalloonTipText = string.Empty;
            Visible = false;
        }

        public void BackupText() => _notifyIcon.BalloonTipText = _backupBaloonTipText;

        public void ShowError(string errorMessage)
        {
            SetNewMessage(errorMessage, ToolTipIcon.Error);
            ShowLastMessage();
            BackupText();
        }
    }
}