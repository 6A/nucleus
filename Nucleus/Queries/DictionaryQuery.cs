using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nucleus
{
    public class DictionaryQuery<T> : BaseQuery<T>, IDictionary<string, T>
    {
        private List<string> keys;

        internal DictionaryQuery(Sector sector, bool saveOnDisposed) : base(sector, saveOnDisposed)
        {
            byte[] bytes;
            if (this.TryRead(0, out bytes))
            {
                keys = Encoding.UTF8.GetString(bytes, 0, bytes.Length).Split(';').ToList();
            }
            else
            {
                keys = new List<string>();
                s.Values.Add(0, 0);
            }
        }

        public int Count { get { return this.keys.Count; } }
        public bool IsReadOnly { get { return false; } }

        public ICollection<string> Keys { get { return keys.ToArray(); } }
        public ICollection<T> Values
        {
            get
            {
                return keys.Select(x => this[x]).ToArray();
            }
        }

        public T this[string key]
        {
            get
            {
                int index = keys.IndexOf(key) + 1;
                byte[] bytes;

                if (index == 0)
                {
                    throw new IndexOutOfRangeException();
                }
                else if (this.TryRead(index, out bytes))
                {
                    return this.Deserialize(index, bytes);
                }
                else
                {
                    throw new IOException();
                }
            }

            set
            {
                int index = keys.IndexOf(key) + 1;

                if (index == 0)
                {
                    throw new KeyNotFoundException();
                }
                else
                {
                    this.TryWrite(index, value);
                }
            }
        }

        public void Add(string key, T value)
        {
            if (keys.Contains(key))
            {
                this[key] = value;
            }
            else if (this.TryWrite(keys.Count + 1, value))
            {
                keys.Add(key);
                this.TryWrite(0, Encoding.UTF8.GetBytes(String.Join(";", keys)));
            }
        }

        public bool ContainsKey(string key)
        {
            return keys.Contains(key);
        }

        public bool Remove(string key)
        {
            int index = keys.IndexOf(key);

            if (index >= 0 && TryRemove(index + 1))
            {
                keys.RemoveAt(index);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryGetValue(string key, out T value)
        {
            if (keys.Contains(key))
            {
                int index = keys.IndexOf(key);
                byte[] bytes;

                if (this.TryRead(index, out bytes))
                {
                    value = this.Deserialize(index, bytes);
                    return true;
                }
                else
                {
                    value = default(T);
                    return false;
                }
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        public void Add(KeyValuePair<string, T> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            if (TryClear())
            {
                keys.Clear();
            }
        }

        public bool Contains(KeyValuePair<string, T> item)
        {
            return keys.Contains(item.Key) ? this[item.Key].Equals(item.Value) : false;
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            foreach (string key in keys)
            {
                array[arrayIndex++] = new KeyValuePair<string, T>(key, this[key]);
            }
        }

        public bool Remove(KeyValuePair<string, T> item)
        {
            return Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            foreach (string key in keys)
            {
                yield return new KeyValuePair<string, T>(key, this[key]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
