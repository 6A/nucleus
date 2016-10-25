using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Imports.Newtonsoft.Json;
using Raven.Imports.Newtonsoft.Json.Bson;

namespace Nucleus.Tests
{
    public class Connection : CoreConnection
    {
        protected override Stream RWStream { get { return fs; } }
        FileStream fs;

        protected override T Deserialize<T>(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            using (BsonReader reader = new BsonReader(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize<T>(reader);
            }
        }

        protected override byte[] Serialize<T>(T obj)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BsonWriter writer = new BsonWriter(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, obj);

                return ms.ToArray();
            }
        }

        public Connection(string url)
        {
            fs = new FileStream(url, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            Initialize();
        }
    }
}
