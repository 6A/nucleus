using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nucleus
{
    internal class OrderedDictionary : IEnumerable<KeyValuePair<int, int>>
    {
        public List<int> Keys { get; private set; }
        public List<int> Values { get; private set; }
        public int Length { get { return Keys.Count; } }

        public OrderedDictionary()
        {
            Keys = new List<int>();
            Values = new List<int>();
        }

        public OrderedDictionary(string data) : this()
        {
            foreach (string s in data.Split(';'))
            {
                string[] split = s.Split(':');

                Keys.Add(int.Parse(split[0]));
                Values.Add(int.Parse(split[1]));
            }
        }

        public int this[int key]
        {
            get
            {
                int index = Keys.IndexOf(key);
                return Values[index];
            }

            set
            {
                int index = Keys.IndexOf(key);
                Values[index] = value;
            }
        }

        public void Remove(int key)
        {
            int index = Keys.IndexOf(key);

            if (index != -1)
            {
                Keys.RemoveAt(index);
                Values.RemoveAt(index);
            }
        }

        public void Add(int key, int value)
        {
            Keys.Add(key);
            Values.Add(value);
        }

        public void PushToEnd(int key, int value)
        {
            int index = Keys.IndexOf(key);

            if (index != -1)
            {
                Keys.RemoveAt(index);
                Values.RemoveAt(index);
            }

            Keys.Add(key);
            Values.Add(value);
        }

        public bool ContainsKey(int key)
        {
            return Keys.Contains(key);
        }

        public IEnumerator<KeyValuePair<int, int>> GetEnumerator()
        {
            int i = 0;
            foreach (int key in Keys)
            {
                yield return new KeyValuePair<int, int>(key, Values[i++]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
