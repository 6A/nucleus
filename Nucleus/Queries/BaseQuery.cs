using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nucleus
{
    public abstract class BaseQuery<T> : IDisposable
    {
        private const int BLOCK_SIZE = 4096;

        internal Sector s;
        internal Dictionary<int, T> cache;
        private MemoryStream io { get { return s.Stream; } }
        bool changed;

        public bool SaveOnDisposed { get; set; }

        internal BaseQuery(Sector sector, bool saveOnDisposed)
        {
            s = sector;
            cache = new Dictionary<int, T>();
            changed = false;
            SaveOnDisposed = saveOnDisposed;

            s.Updated += SectorUpdated;
            s.Connected++;
        }

        public override string ToString()
        {
            return io == null ? "Disposed." : Encoding.UTF8.GetString(io.ToArray(), 0, (int)io.Length);
        }

        private void SectorUpdated(object obj)
        {
            cache = obj as Dictionary<int, T>;
        }

        public void Save()
        {
            if (changed)
            {
                s.Update();
                changed = false;
            }
        }

        protected void Write(int index, byte[] obj)
        {
            if (s.Values.ContainsKey(index))
            {
                int oldLength = s.Values[index];
                int ioffset = s.Values.TakeWhile(x => x.Key != index).Sum(x => x.Value);
                int diff = obj.Length - oldLength;

                if (diff == 0) // same size
                {
                    io.Seek(ioffset, SeekOrigin.Begin);
                    io.Write(obj, 0, obj.Length);
                }
                else
                {
                    // overwrite old object by rest of stream
                    byte[] bytes = new byte[BLOCK_SIZE];
                    int read = 0;
                    long left = io.Length - ioffset - oldLength;

                    io.Seek(ioffset + oldLength, SeekOrigin.Begin);
                    do
                    {
                        read = io.Read(bytes, 0, bytes.Length);
                        io.Seek(-(oldLength + read), SeekOrigin.Current);
                        io.Write(bytes, 0, read);
                    }
                    while ((left -= read) > 0);

                    if (diff < 0) // shrink stream
                        io.SetLength(io.Length + diff);

                    // write object after overriden data
                    io.Write(obj, 0, obj.Length);
                    s.Values.PushToEnd(index, obj.Length);
                }
            }
            else
            {
                io.Seek(0, SeekOrigin.End);
                io.Write(obj, 0, obj.Length);

                s.Values.Add(index, obj.Length);
            }

            changed = true;
            s.enumerator.UpdateSector(s, this.cache);
        }

        protected bool TryWrite(int index, byte[] obj)
        {
            try
            {
                Write(index, obj);
                cache.Remove(index);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        protected bool TryWrite(int index, T obj)
        {
            try
            {
                byte[] bytes = s.enumerator.core.PreSerialize<T>(obj);
                Write(index, bytes);
                cache[index] = obj;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        protected bool TryRemove(int index)
        {
            try
            {
                if (s.Values.ContainsKey(index))
                {
                    int oldLength = s.Values[index];
                    int ioffset = s.Values.TakeWhile(x => x.Key != index).Sum(x => x.Value);

                    if (oldLength > 0)
                    {
                        // overwrite old object by rest of stream
                        byte[] bytes = new byte[BLOCK_SIZE];
                        int read = 0;
                        long left = io.Length - ioffset - oldLength;

                        io.Seek(ioffset + oldLength, SeekOrigin.Begin);
                        do
                        {
                            read = io.Read(bytes, 0, bytes.Length);
                            io.Seek(-(oldLength + read), SeekOrigin.Current);
                            io.Write(bytes, 0, read);
                        }
                        while ((left -= read) > 0);

                        io.SetLength(io.Length - oldLength);
                    }

                    changed = true;
                    cache.Remove(index);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        protected bool TryClear()
        {
            try
            {
                io.SetLength(0);
                cache.Clear();
                changed = true;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        protected bool TryRead(int index, out byte[] buffer)
        {
            if (!s.Values.ContainsKey(index))
            {
                buffer = new byte[0];
                return false;
            }
            else
            {
                int ioffset = s.Values.TakeWhile(x => x.Key != index).Sum(x => x.Value);
                int length = s.Values[index];

                io.Seek(ioffset, SeekOrigin.Begin);

                buffer = new byte[length];
                return io.Read(buffer, 0, length) == length;
            }
        }

        protected T Deserialize(int index, byte[] bytes)
        {
            if (!cache.ContainsKey(index))
                cache.Add(index, (T)s.enumerator.core.PreDeserialize<T>(bytes));
            return cache[index];
        }
        
        protected object Deserialize(int index, byte[] bytes, Type t)
        {
            if (!cache.ContainsKey(index))
                cache.Add(index, (T)s.enumerator.core.PreDeserialize(bytes, t));
            return cache[index];
        }

        protected void ClearCache()
        {
            cache.Clear();
        }

        public void Dispose()
        {
            if (this.SaveOnDisposed)
                this.Save();

            cache = null;
            s.Connected--;
        }
    }
}
