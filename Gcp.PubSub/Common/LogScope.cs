using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gcp.PubSub.Common;
using Microsoft.Extensions.Logging;

namespace Gcp.PubSub.Common
{
    public class LogScope : IDisposable
    {
        private string Dump(object obj, string description) => _serializer.ToJson(obj, description);

        private readonly LogLevel _logLevel;
        private readonly DateTime _start;
        private readonly IObjectSerializer _serializer;
        private bool _disposed;
        private readonly ILogger _logger;
        private readonly string _description;

        public LogScope(ILogger logger, IObjectSerializer serializer, object obj = null, string desc = null, LogLevel logLevel = LogLevel.Information)
        {
            _logLevel = logLevel;
            _start = DateTime.Now;
            _logger = logger;
            _description = desc;
            _serializer = serializer;

            if (_logger.IsEnabled(_logLevel))
            {
                var text = $"Begin {_description}";

                if (obj != null)
                {
                    text = Dump(obj, text);
                }

                logger.Log(logLevel, text);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_logger.IsEnabled(_logLevel))
                {
                    var duration = DateTime.Now.Subtract(_start);
                    var vars = new Dictionary<string, object>()
                    {
                        {"elapsed", (int) duration.TotalMilliseconds}
                    };
                    using (_logger.BeginScope(vars))
                    {
                        _logger.Log(_logLevel, $"Finished {_description} ({(int)duration.TotalMilliseconds} ms)");
                    }
                }
            }
            _disposed = true;
        }
	}
}
internal static class LogScopeExtensions
{
    public static LogScope CreateScope(this ILogger logger, object obj = null, string desc = null) => new(logger, new ObjectSerializer(), obj, desc);
}
