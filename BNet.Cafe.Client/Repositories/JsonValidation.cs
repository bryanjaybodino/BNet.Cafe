using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Cafe.Client.Repositories
{
    internal class JsonValidation
    {
        public static bool IsValidJson(string textMessage)
        {
            if (string.IsNullOrWhiteSpace(textMessage)) return false;

            textMessage = textMessage.Trim();

            // JSON must start and end with { } (for objects) or [ ] (for arrays)
            if ((textMessage.StartsWith("{") && textMessage.EndsWith("}")) ||
                (textMessage.StartsWith("[") && textMessage.EndsWith("]")))
            {
                try
                {
                    JToken.Parse(textMessage);
                    return true;
                }
                catch (JsonReaderException)
                {
                    return false;
                }
            }

            return false;
        }
    }
}
