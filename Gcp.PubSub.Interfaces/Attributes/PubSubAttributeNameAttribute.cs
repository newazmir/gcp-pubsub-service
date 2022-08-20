using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gcp.PubSub.Interfaces.Attributes
{
    public class PubSubAttributeNameAttribute : Attribute
    {
        public PubSubAttributeNameAttribute(string name, bool serializableToPayload = false)
        {
            Name = name;
            SerializableToPayload = serializableToPayload;
        }

        public string Name { get; }
        public bool SerializableToPayload { get; }
    }
}
