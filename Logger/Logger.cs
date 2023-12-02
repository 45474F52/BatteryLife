namespace BatteryLife.Logger
{
    public abstract class Logger
    {
        public abstract void Log(LogTypes type, string message, bool rewrite = false);
    }
}