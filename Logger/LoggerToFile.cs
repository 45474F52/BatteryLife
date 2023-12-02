using System;
using System.IO;
using System.Text;

namespace BatteryLife.Logger
{
    public class LoggerToFile : Logger
    {
        private readonly string _pathToFile;

        public LoggerToFile(string pathToFile) => _pathToFile = pathToFile;

        public override void Log(LogTypes type, string message, bool rewrite = false)
        {
            LogAsync(type, message, rewrite);
        }

        private async void LogAsync(LogTypes type, string message, bool rewrite)
        {
            FileMode fileMode;
            if (File.Exists(_pathToFile))
                fileMode = rewrite ? FileMode.Open : FileMode.Append;
            else
                fileMode = FileMode.CreateNew;

            FileStream stream = new FileStream(_pathToFile, fileMode, FileAccess.Write, FileShare.None);

            try
            {
                byte[] buffer = Encoding.Default.GetBytes($"{type}: [{DateTime.Now:G}]\n{message}\n");
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch
            {
                throw;
            }
            finally
            {
                stream.Close();
            }
        }
    }
}