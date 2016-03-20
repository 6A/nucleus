using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nucleus.Tests
{
    public class Connection : CoreConnection
    {
        FileStream fs;

        protected override T Deserialize<T>(byte[] bytes)
        {
            if (typeof(T) == typeof(String))
            {
                return (T)(object)Encoding.UTF8.GetString(bytes);
            }
            else if (typeof(T) == typeof(DateTime))
            {
                return (T)(object)DateTime.FromBinary(BitConverter.ToInt64(bytes, 0));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        protected override Stream GetRWStream()
        {
            return fs;
        }

        protected override byte[] Serialize<T>(T obj)
        {
            return obj is DateTime ? BitConverter.GetBytes(((DateTime)(object)obj).ToBinary()) : Encoding.UTF8.GetBytes(obj.ToString());
        }

        public Connection(string url)
        {
            fs = new FileStream(url, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            Initialize();
        }
    }
}
