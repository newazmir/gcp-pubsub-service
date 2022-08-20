using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Gcp.PubSub.Common
{
    public class ObjectSerializer : IObjectSerializer
    {
        public string ToJson(object obj, string comment = "", bool truncate = true)
        {
            if (obj == null)
            {
                return $"{comment}";
            }

            var dump = JsonSerializer.Serialize(obj);

            if (dump.Length > 2000 && truncate)
            {
                var truncated = string.Concat(dump.AsSpan(0, 1000), "[***TRUNCATED***]",
                    dump.AsSpan(dump.Length - 1001, 1000));
                dump = $"{comment}{Environment.NewLine}{truncated}";
            }
            else
            {
                dump = $"{comment}{Environment.NewLine}{dump}";
            }

            return dump;
        }
    }
}
