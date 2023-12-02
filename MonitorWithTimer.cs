using System;
using System.Windows.Forms;

namespace BatteryLife
{
    public sealed class MonitorWithTimer
    {
        private readonly Timer _timer;

        public delegate void Tick(object sender, EventArgs e);

        public event Tick TimerTick;

        public MonitorWithTimer(Timer timer)
        {
            _timer = timer;
            _timer.Tick += (object sender, EventArgs e) => TimerTick?.Invoke(sender, e);
        }

        public void StartMonitoring(int interval)
        {
            _timer.Interval = interval;
            _timer.Start();
        }

        public void StopMonitoring() => _timer.Stop();
    }
}