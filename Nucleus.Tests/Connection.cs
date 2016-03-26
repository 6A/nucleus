using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Imports.Newtonsoft.Json;

namespace Nucleus.Tests
{
    public class Connection : CoreConnection
    {
        FileStream fs;

        protected override T Deserialize<T>(byte[] bytes)
        {
            return typeof(T) == typeof(string)
                ? (T)(object)Encoding.UTF8.GetString(bytes)
                : JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes));
        }

        protected override Stream GetRWStream()
        {
            return fs;
        }

        protected override byte[] Serialize<T>(T obj)
        {
            return typeof(T) == typeof(string)
                ? Encoding.UTF8.GetBytes((string)(object)obj)
                : Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
        }

        public Connection(string url)
        {
            fs = new FileStream(url, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            Initialize();
        }
    }
}
