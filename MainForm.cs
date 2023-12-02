using BatteryLife.Logger;
using BatteryLife.Serializer;
using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;

namespace BatteryLife
{
    public partial class MainForm : Form
    {
        private const int NotifyIconTimeoutInMilliseconds = 5000;

        private static readonly string _pathToLog = string.Empty;
        private static readonly string _pathToSave = string.Empty;

        private readonly Logger.Logger _logger;
        private readonly Serializer<int[]> _serializer;
        private readonly NotifyIconHandler _notifyIconHandler;
        private readonly MonitorWithTimer _monitor;

        private int _tickAmount;
        public int TickAmount
        {
            get => _tickAmount;
            set
            {
                _tickAmount = value;
                numTickAmount.Text = value.ToString();
            }
        }

        private int _criticalPercents;
        public int CriticalPercents
        {
            get => _criticalPercents;
            set
            {
                _criticalPercents = value;
                numCriticalPercent.Text = value.ToString();
            }
        }

        public new void Show()
        {
            Application.Run(this);
        }

        static MainForm()
        {
            _pathToLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BatteryObserverLog.txt");
            _pathToSave = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves.json");
        }

        public MainForm()
        {
            InitializeComponent();
            
            this.MaximizeBox = false;

            _logger = new LoggerToFile(_pathToLog);

            _serializer = new JSONSerializer<int[]>(_pathToSave);

            _notifyIconHandler = new NotifyIconHandler(ref notifyIcon, NotifyIconTimeoutInMilliseconds);

            notifyIcon.MouseClick += (object sender, MouseEventArgs e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    _notifyIconHandler.SetNewMessage($"Осталось {BatteryLifeDiagram.Value}%", ToolTipIcon.Info);
                    _notifyIconHandler.ShowLastMessage();
                }
            };

            notifyIcon.MouseDoubleClick += (object sender, MouseEventArgs e) =>
            {
                if (e.Button == MouseButtons.Left)
                    ShowApp();
            };

            notifyIcon.BalloonTipClicked += OnBaloonTipClicked;

            _monitor = new MonitorWithTimer(new Timer());

            _monitor.TimerTick += (object sender, EventArgs e) =>
            {
                if (SystemBatteryHandler.CurrentPercents <= _criticalPercents)
                {
                    _notifyIconHandler.ShowMessage("Критический уровень заряда батареи!", ToolTipIcon.Warning);
                    _monitor.StopMonitoring();
                }
            };

            GetPropertiesFromSaves();
        }

        private void OnBaloonTipClicked(object sender, EventArgs e)
        {
            ShowApp();
        }

        private void Form1_Load(object sender, EventArgs e) => RefreshDataView();

        private void RefreshDataView()
        {
            ChangePercentDiagram();
            SetPropertiesText();
        }

        private void SetPropertiesText()
        {
            lProps.Text =
                $"Название: {SystemBatteryHandler.BatteryProperties.Single(p => p.Name.ToLower() == "name").Value ?? "NULL"}\n" +
                $"Идентификатор: {SystemBatteryHandler.BatteryProperties.Single(p => p.Name.ToLower() == "deviceid").Value ?? "NULL"}\n" +
                $"Напряжение (мВ): {SystemBatteryHandler.BatteryProperties.Single(p => p.Name.ToLower() == "designvoltage").Value ?? "NULL"}\n" +
                $"Статус: {SystemBatteryHandler.BatteryProperties.Single(p => p.Name.ToLower() == "status").Value ?? "NULL"}\n" +
                $"\n{SystemBatteryHandler.BatteryProperties.Single(p => p.Name.ToLower() == "lasterrorcode").Value}" +
                $"{SystemBatteryHandler.BatteryProperties.Single(p => p.Name.ToLower() == "errordescription").Value}\n" +
                $"\n{SystemBatteryHandler.BatteryProperties.Single(p => p.Name.ToLower() == "configmanagererrorcode").Value}\n";
        }

        private void ChangePercentDiagram()
        {
            BatteryChargeStatus status = SystemInformation.PowerStatus.BatteryChargeStatus;

            BatteryLifeDiagram.Value = (int)Math.Round(SystemBatteryHandler.CurrentPercents, MidpointRounding.AwayFromZero);
            SetDiagramColor(status);
            BatteryLifeDiagram.Text = $"{BatteryLifeDiagram.Value}%";

            lStatus.Text = status.ToString().Replace(", ", Environment.NewLine);
        }

        private void SetDiagramColor(BatteryChargeStatus status)
        {
            switch (status)
            {
                case BatteryChargeStatus.High:
                case BatteryChargeStatus.High | BatteryChargeStatus.Charging:
                    BatteryLifeDiagram.ProgressColor = Color.SpringGreen;
                    break;
                case BatteryChargeStatus.Low:
                case BatteryChargeStatus.Low | BatteryChargeStatus.Charging:
                    BatteryLifeDiagram.ProgressColor = Color.Orange;
                    break;
                case BatteryChargeStatus.Critical:
                case BatteryChargeStatus.Critical | BatteryChargeStatus.Charging:
                    BatteryLifeDiagram.ProgressColor = Color.OrangeRed;
                    break;
                case BatteryChargeStatus.NoSystemBattery:
                case BatteryChargeStatus.Unknown:
                    BatteryLifeDiagram.ProgressColor = Color.Transparent;
                    break;
                case BatteryChargeStatus.Charging:
                    BatteryLifeDiagram.ProgressColor = Color.DeepSkyBlue;
                    break;
                default:
                    BatteryLifeDiagram.ProgressColor = Color.Black;
                    break;
            }

            lStatus.ForeColor = BatteryLifeDiagram.ProgressColor;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            SystemBatteryHandler.UpdateBatteryProperties();
            RefreshDataView();
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            try
            {
                _logger.Log(LogTypes.Debug, SystemBatteryHandler.PropertiesToString);
            }
            catch (Exception ex)
            {
                _notifyIconHandler.ShowError($"Ошибка при логировании:\n{ex.Message}");
                return;
            }

            notifyIcon.BalloonTipClicked += GoToLogFile;
            notifyIcon.BalloonTipClosed += OnBalloonTipClosed;
            _notifyIconHandler.ShowMessage(
                $"Информация залогирована по пути \"{_pathToLog}\"",
                ToolTipIcon.Info);
        }

        private void OnBalloonTipClosed(object sender, EventArgs e)
        {
            notifyIcon.BalloonTipClicked -= GoToLogFile;
            notifyIcon.BalloonTipClosed -= OnBalloonTipClosed;
        }

        private void GoToLogFile(object sender, EventArgs e)
        {
            Process.Start(_pathToLog);
            notifyIcon.BalloonTipClicked -= GoToLogFile;
            notifyIcon.BalloonTipClosed -= OnBalloonTipClosed;
        }

        private void HideApp()
        {
            this.ShowInTaskbar = false;
            _notifyIconHandler.Visible = true;
            _notifyIconHandler.SetNewMessage($"Осталось {BatteryLifeDiagram.Value}%", ToolTipIcon.Info);

            ParseValues();

            if (_tickAmount >= 1)
                _monitor.StartMonitoring(_tickAmount);
        }

        private void ParseValues()
        {
            int.TryParse(numTickAmount.Text, out int tickAmount);
            TickAmount = tickAmount;
            int.TryParse(numCriticalPercent.Text, out int criticalPercent);
            CriticalPercents = criticalPercent;
        }

        private void ShowApp()
        {
            if (!this.ShowInTaskbar)
            {
                this.ShowInTaskbar = true;
                this.WindowState = FormWindowState.Normal;
                _notifyIconHandler.ResetProperties();
                _monitor.StopMonitoring();
                SystemBatteryHandler.UpdateBatteryProperties();
                RefreshDataView();
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                HideApp();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            ParseValues();

            try
            {
                _serializer.Serialize(new int[2] { _tickAmount, _criticalPercents });
            }
            catch (Exception ex)
            {
                _notifyIconHandler.ShowError($"Ошибка при сериализации данных:\n{ex.Message}");
            }
        }

        private void GetPropertiesFromSaves()
        {
            int[] values = _serializer.Deserialize();

            if (values != null)
            {
                TickAmount = values[0];
                CriticalPercents = values[1];
            }
        }
    }
}