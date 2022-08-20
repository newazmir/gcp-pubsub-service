using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gcp.PubSub.Interfaces.Enums;

namespace Gcp.PubSub.Interfaces
{
    public interface IPubSubResponse : IPubSubMessageBase
    {
        PubSubResponseStatusCode StatusCode { get; }
    }

    public interface IPubSubResponse<out TPayload, out TKey> : IPubSubResponse
    {
        TPayload Payload { get; }
        TKey Key { get; }
    }
}
