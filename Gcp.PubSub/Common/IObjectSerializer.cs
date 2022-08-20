using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gcp.PubSub.Common
{
    public interface IObjectSerializer
    {
        string ToJson(object obj, string comment = "", bool truncate = true);
    }
}
