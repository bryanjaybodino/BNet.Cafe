using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

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
                    var serializer = new JavaScriptSerializer();
                    serializer.DeserializeObject(textMessage);
                    return true;
                }
                catch (ArgumentException)
                {
                    return false;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }

            return false;
        }
    }
}