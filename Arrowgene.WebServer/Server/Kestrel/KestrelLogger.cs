using System;
using Arrowgene.Logging;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Arrowgene.WebServer.Server.Kestrel
{
    public class KestrelLogger : ILogger
    {
        private static readonly Arrowgene.Logging.ILogger Logger = LogProvider.Logger(typeof(KestrelLogger));

        private readonly string _name;

        public KestrelLogger(string name)
        {
            _name = name;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (exception != null)
            {
                Logger.Exception(exception);
                return;
            }

            string message = $"{formatter(state, null)}";
            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.None:
                case LogLevel.Information:
                    Logger.Debug(message);
                    break;
                case LogLevel.Warning:
                    Logger.Info(message);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    Logger.Error(message);
                    break;
            }
        }
    }
}