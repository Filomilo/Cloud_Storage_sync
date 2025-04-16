using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cloud_Storage_Common
{
    public static class JsonOperations
    {
        public static string jsonFromObject(object obj)
        {
            string json = JsonSerializer.Serialize(obj);
            return json;
        }

        public static T ObjectFromJSon<T>(string json)
        {
            T obj = JsonSerializer.Deserialize<T>(json);
            return obj;
        }
    }
}
