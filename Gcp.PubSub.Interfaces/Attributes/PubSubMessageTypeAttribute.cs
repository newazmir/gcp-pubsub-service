using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gcp.PubSub.Interfaces.Attributes
{
    /// <summary>
    /// This is an alternative way to provide message type and version to Request/Response message
    /// Apply this attribute to the Request/Response class to override Message Type
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PubSubMessageTypeAttribute : Attribute
    {
        public PubSubMessageTypeAttribute(string messageType, string messageVersion)
        {
            if (messageType == null)
            {
                throw new ArgumentNullException(nameof(messageType));
            }

            if (messageVersion == null)
            {
                throw new ArgumentNullException(nameof(messageVersion));
            }

            MessageType = PubSubMessageType.Create(messageType, messageVersion);
        }

        public PubSubMessageTypeAttribute(string messageType)
        {
            if (messageType == null)
            {
                throw new ArgumentNullException(nameof(messageType));
            }

            MessageType = PubSubMessageType.Create(messageType, null);
        }

        public PubSubMessageType MessageType { get; }
    }
}
