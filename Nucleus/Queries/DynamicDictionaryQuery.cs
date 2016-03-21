using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nucleus
{
    public class DynamicDictionaryKeyValuePair
    {
        private object value;
        private Func<object> loader;

        public Type ValueType { get; private set; }
        public string Key { get; private set; }

        public object Value
        {
            get
            {
                if (value == null)
                    value = loader();

                return value;
            }
        }

        internal DynamicDictionaryKeyValuePair(Type t, Func<object> f, string k)
        {
            loader = f;
            ValueType = t;
            Key = k;
        }

        public override string ToString()
        {
            return String.Format("[{0}] {1} = {2}", ValueType, Key, Value);
        }
    }

    public class DynamicDictionaryQuery : BaseQuery<object>, IEnumerable<DynamicDictionaryKeyValuePair>
    {
        private List<string> keys;
        private List<Type> types;

        internal DynamicDictionaryQuery(Sector sector, bool saveOnDisposed) : base(sector, saveOnDisposed)
        {
            byte[] keysBytes;
            byte[] typesBytes;

            if (this.TryRead(0, out keysBytes) && this.TryRead(1, out typesBytes))
            {
                keys = Encoding.UTF8.GetString(keysBytes, 0, keysBytes.Length).Split(';').ToList();
                types = Encoding.UTF8.GetString(typesBytes, 0, typesBytes.Length).Split(';').Select(x => Type.GetType(x)).ToList();
            }
            else
            {
                keys = new List<string>();
                types = new List<Type>();
                s.Values.Add(0, 0);
                s.Values.Add(1, 0);
            }
        }

        public int Count { get { return this.keys.Count; } }

        private int IndexOf(string key, Type t)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i] == key && types[i] == t)
                {
                    return i + 2;
                }
            }
            return -1;
        }

        private bool TryGet(string key, Type t, out object o)
        {
            o = null;
            int i = IndexOf(key, t);

            if (i > 0)
            {
                byte[] buffer;

                if (TryRead(i, out buffer))
                {
                    o = cache.ContainsKey(i) ? cache[i] : this.Deserialize(i, buffer, types[i - 2]);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public T Get<T>(string key)
        {
            object o;

            if (TryGet(key, typeof(T), out o))
            {
                return (T)o;
            }
            else
            {
                throw new KeyNotFoundException();
            }
        }

        public bool Contains<T>(string key)
        {
            return IndexOf(key, typeof(T)) != -1;
        }

        public void Set<T>(string key, T value)
        {
            int index = IndexOf(key, typeof(T));

            if (index > 0)
            {
                TryWrite(index, value);
            }
            else if (this.TryWrite(keys.Count + 2, value))
            {
                keys.Add(key);
                types.Add(typeof(T));
                this.TryWrite(0, Encoding.UTF8.GetBytes(String.Join(";", keys)));
                this.TryWrite(1, Encoding.UTF8.GetBytes(String.Join(";", types)));
            }
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            object o;

            if (TryGet(key, typeof(T), out o))
            {
                value = (T)o;
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        public void Remove<T>(string key)
        {
            int index = IndexOf(key, typeof(T));

            if (index > 0)
            {
                if (TryRemove(index))
                {
                    types.RemoveAt(index);
                    keys.RemoveAt(index);
                }
            }
        }

        public void Clear()
        {
            if (TryClear())
            {
                types.Clear();
                keys.Clear();
            }
        }

        public IEnumerator<DynamicDictionaryKeyValuePair> GetEnumerator()
        {
            int i = 0;
            foreach (var kvp in s.Values.Skip(2))
            {
                yield return new DynamicDictionaryKeyValuePair(types[i], () =>
                {
                    object o;
                    if (TryGet(keys[i], types[i], out o))
                    {
                        return o;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }, keys[i]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
