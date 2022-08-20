using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gcp.PubSub.Interfaces.Attributes
{
    public class PubSubMessageType
    {
        public string Type { get; }
        public string? Version { get; }

        private PubSubMessageType(string type, string? version)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentNullException(nameof(type));
            }

            Type = type;
            Version=version;
        }

        public override string ToString() => $"{Type}-{Version}";

        public static PubSubMessageType Create(string messageType, string? messageVersion) => new PubSubMessageType(messageType, messageVersion);

        public static PubSubMessageType Create(string messageType) => new PubSubMessageType(messageType, null);
    }
}
