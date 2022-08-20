using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gcp.PubSub.Interfaces
{
    public interface IPubSubMessageBase
    {
        string? CorrelationId { get; set; }
        string? Originator { get; set; }
    }
}
