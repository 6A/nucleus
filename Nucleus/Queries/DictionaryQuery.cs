using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
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
                value = this[key];
                return true;
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

        #region Dynamic
        public dynamic AsDynamic()
        {
            return new DynamicQuery<T>(this);
        }

        public class DynamicQuery<T> : DynamicObject, IDisposable
        {
            private DictionaryQuery<T> query;

            public DynamicQuery(DictionaryQuery<T> dic)
            {
                query = dic;
            }

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return query.keys;
            }

            private string keyIn(object[] indexes)
            {
                return indexes.Length == 1 && indexes[0] is string ? (string)indexes[0] : null;
            }

            public override bool TryDeleteIndex(DeleteIndexBinder binder, object[] indexes)
            {
                string s = keyIn(indexes);
                return (s == null) ? false : query.Remove(s);
            }

            public override bool TryDeleteMember(DeleteMemberBinder binder)
            {
                return query.Remove(binder.Name);
            }

            public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
            {
                string s = keyIn(indexes);

                if (s != null && query.keys.Contains(s))
                {
                    result = query[s];
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                if (query.keys.Contains(binder.Name))
                {
                    result = query[binder.Name];
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }

            public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
            {
                string s = keyIn(indexes);

                if (s != null && value is T)
                {
                    query[s] = (T)value;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                if (value is T)
                {
                    query[binder.Name] = (T)value;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Dispose()
            {
                query.Dispose();
            }
        }
        #endregion
    }
}
