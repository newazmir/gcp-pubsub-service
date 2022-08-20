using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Gcp.PubSub.Common
{
    public class LogScopeFactory
    {
        public IObjectSerializer Serializer { get;}
        public LogScopeFactory(IObjectSerializer serializer) => Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        public LogScope CreateScope(ILogger logger, object obj = null, string desc = null, LogLevel logLevel = LogLevel.Information)
            => new(logger, Serializer, obj, desc, logLevel);
        public static readonly LogScopeFactory Default = new(new ObjectSerializer());
    }
}
