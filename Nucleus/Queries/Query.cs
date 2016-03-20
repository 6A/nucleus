using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Nucleus
{
    public class Query<T> : BaseQuery<T>, IList<T>
    {
        public int Count { get { return s.Values.Length; } }
        public bool IsReadOnly { get { return false; } }

        internal Query(Sector sector, bool saveOnDisposed) : base(sector, saveOnDisposed) { }

        public T this[int index]
        {
            get
            {
                byte[] bytes;

                if (index == -1 || index >= Count)
                {
                    throw new IndexOutOfRangeException("index");
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
                if (index == -1 || index > Count)
                {
                    throw new IndexOutOfRangeException("index");
                }
                else
                {
                    this.TryWrite(index, value);
                }
            }
        }

        public void Add(T item)
        {
            this[Count] = item;
        }

        public void Insert(int index, T item)
        {
            this[index] = item;
        }

        public int IndexOf(T item)
        {
            foreach (var kvp in s.Values)
            {
                if (this[kvp.Key].Equals(item))
                    return kvp.Key;
            }
            return -1;
        }

        public void RemoveAt(int index)
        {
            TryRemove(index);
        }

        public void Clear()
        {
            TryClear();
        }

        public bool Contains(T item)
        {
            foreach (T t in this)
            {
                if (t.Equals(item))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var kvp in s.Values)
            {
                array[arrayIndex++] = this[kvp.Key];
            }
        }

        public bool Remove(T item)
        {
            foreach (var kvp in s.Values)
            {
                if (this[kvp.Key].Equals(item)) return TryRemove(kvp.Key);
            }
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var kvp in s.Values)
            {
                yield return this[kvp.Key];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
