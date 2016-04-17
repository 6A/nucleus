using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Nucleus
{
    /// <summary>
    /// Establish a connection to a file database.
    /// </summary>
    public abstract class CoreConnection : IDisposable
    {
        #region Abstract members
        /// <summary>
        /// Read / Write Stream to the file that contains the database.
        /// </summary>
        protected abstract Stream RWStream { get; }

        /// <summary>
        /// Automatically serialize numeric values, strings, chars, bytes, DateTime, TimeSpan and arrays of numeric values
        /// </summary>
        internal protected virtual byte[] PreSerialize<T>(T obj)
        {
            if (obj == null)
                return new byte[0];
            else if (obj is short)
                return BitConverter.GetBytes((short)(object)obj);
            else if (obj is int)
                return BitConverter.GetBytes((int)(object)obj);
            else if (obj is long)
                return BitConverter.GetBytes((long)(object)obj);
            else if (obj is ushort)
                return BitConverter.GetBytes((ushort)(object)obj);
            else if (obj is uint)
                return BitConverter.GetBytes((uint)(object)obj);
            else if (obj is ulong)
                return BitConverter.GetBytes((ulong)(object)obj);
            else if (obj is float)
                return BitConverter.GetBytes((float)(object)obj);
            else if (obj is double)
                return BitConverter.GetBytes((double)(object)obj);
            else if (obj is bool)
                return BitConverter.GetBytes((bool)(object)obj);
            else if (obj is char)
                return BitConverter.GetBytes((char)(object)obj);
            else if (obj is string)
                return Encoding.UTF8.GetBytes((string)(object)obj);
            else if (obj is byte)
                return new byte[] { (byte)(object)obj };
            else if (obj is IntPtr)
                return BitConverter.GetBytes(((IntPtr)(object)obj).ToInt64());
            else if (obj is DateTime)
                return BitConverter.GetBytes(((DateTime)(object)obj).ToBinary());
            else if (obj is TimeSpan)
                return BitConverter.GetBytes(((TimeSpan)(object)obj).Ticks);
            else if (obj is short[])
                return ((short[])(object)obj).SelectMany(x => BitConverter.GetBytes(x)).ToArray();
            else if (obj is int[])
                return ((int[])(object)obj).SelectMany(x => BitConverter.GetBytes(x)).ToArray();
            else if (obj is long[])
                return ((long[])(object)obj).SelectMany(x => BitConverter.GetBytes(x)).ToArray();
            else if (obj is ushort[])
                return ((ushort[])(object)obj).SelectMany(x => BitConverter.GetBytes(x)).ToArray();
            else if (obj is uint[])
                return ((uint[])(object)obj).SelectMany(x => BitConverter.GetBytes(x)).ToArray();
            else if (obj is ulong[])
                return ((ulong[])(object)obj).SelectMany(x => BitConverter.GetBytes(x)).ToArray();
            else if (obj is float[])
                return ((float[])(object)obj).SelectMany(x => BitConverter.GetBytes(x)).ToArray();
            else if (obj is double[])
                return ((double[])(object)obj).SelectMany(x => BitConverter.GetBytes(x)).ToArray();
            else if (obj is bool[])
                return ((bool[])(object)obj).SelectMany(x => BitConverter.GetBytes(x)).ToArray();
            else if (obj is char[])
                return ((char[])(object)obj).SelectMany(x => BitConverter.GetBytes(x)).ToArray();
            else if (obj is byte[])
                return (byte[])(object)obj;
            else if (obj is IntPtr[])
                return ((IntPtr[])(object)obj).SelectMany(x => BitConverter.GetBytes(x.ToInt64())).ToArray();
            else
                return this.Serialize<T>(obj);
        }

        internal protected virtual object PreDeserialize(byte[] bytes, Type target)
        {
            int i = 0;

            if (target == typeof(Int16))
                return BitConverter.ToInt16(bytes, 0);
            else if (target == typeof(Int32))
                return BitConverter.ToInt32(bytes, 0);
            else if (target == typeof(Int64))
                return BitConverter.ToInt64(bytes, 0);
            else if (target == typeof(UInt16))
                return BitConverter.ToUInt16(bytes, 0);
            else if (target == typeof(UInt32))
                return BitConverter.ToUInt32(bytes, 0);
            else if (target == typeof(UInt64))
                return BitConverter.ToUInt64(bytes, 0);
            else if (target == typeof(Single))
                return BitConverter.ToSingle(bytes, 0);
            else if (target == typeof(Double))
                return BitConverter.ToDouble(bytes, 0);
            else if (target == typeof(Boolean))
                return BitConverter.ToBoolean(bytes, 0);
            else if (target == typeof(Char))
                return BitConverter.ToChar(bytes, 0);
            else if (target == typeof(String))
                return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            else if (target == typeof(IntPtr))
                return new IntPtr(BitConverter.ToInt64(bytes, 0));
            else if (target == typeof(byte))
                return bytes[0];
            else if (target == typeof(Int16[]))
                return new bool[bytes.Length / sizeof(Int16)].Select(x => BitConverter.ToInt16(bytes, i += 4)).ToArray();
            else if (target == typeof(Int32[]))
                return new bool[bytes.Length / sizeof(Int32)].Select(x => BitConverter.ToInt32(bytes, i += 4)).ToArray();
            else if (target == typeof(Int64[]))
                return new bool[bytes.Length / sizeof(Int64)].Select(x => BitConverter.ToInt64(bytes, i += 4)).ToArray();
            else if (target == typeof(UInt16[]))
                return new bool[bytes.Length / sizeof(UInt16)].Select(x => BitConverter.ToUInt16(bytes, i += 4)).ToArray();
            else if (target == typeof(UInt32[]))
                return new bool[bytes.Length / sizeof(UInt32)].Select(x => BitConverter.ToUInt32(bytes, i += 4)).ToArray();
            else if (target == typeof(UInt64[]))
                return new bool[bytes.Length / sizeof(UInt64)].Select(x => BitConverter.ToUInt64(bytes, i += 4)).ToArray();
            else if (target == typeof(Single[]))
                return new bool[bytes.Length / sizeof(Single)].Select(x => BitConverter.ToSingle(bytes, i += 4)).ToArray();
            else if (target == typeof(Double[]))
                return new bool[bytes.Length / sizeof(Double)].Select(x => BitConverter.ToDouble(bytes, i += 4)).ToArray();
            else if (target == typeof(Boolean[]))
                return new bool[bytes.Length / sizeof(Boolean)].Select(x => BitConverter.ToBoolean(bytes, i += 4)).ToArray();
            else if (target == typeof(Char[]))
                return new bool[bytes.Length / sizeof(Char)].Select(x => BitConverter.ToChar(bytes, i += 4)).ToArray();
            else if (target == typeof(IntPtr[]))
                return new bool[bytes.Length / sizeof(Int64)].Select(x => new IntPtr(BitConverter.ToInt64(bytes, i += 4))).ToArray();
            else if (target == typeof(byte[]))
                return bytes;
            else if (target == typeof(DateTime))
                return DateTime.FromBinary(BitConverter.ToInt64(bytes, 0));
            else if (target == typeof(TimeSpan))
                return TimeSpan.FromTicks(BitConverter.ToInt64(bytes, 0));
            else
                return this.Deserialize(bytes, target);
        }

        internal protected virtual object PreDeserialize<T>(byte[] bytes)
        {
            Type target = typeof(T);
            int i = 0;

            if (target == typeof(Int16))
                return BitConverter.ToInt16(bytes, 0);
            else if (target == typeof(Int32))
                return BitConverter.ToInt32(bytes, 0);
            else if (target == typeof(Int64))
                return BitConverter.ToInt64(bytes, 0);
            else if (target == typeof(UInt16))
                return BitConverter.ToUInt16(bytes, 0);
            else if (target == typeof(UInt32))
                return BitConverter.ToUInt32(bytes, 0);
            else if (target == typeof(UInt64))
                return BitConverter.ToUInt64(bytes, 0);
            else if (target == typeof(Single))
                return BitConverter.ToSingle(bytes, 0);
            else if (target == typeof(Double))
                return BitConverter.ToDouble(bytes, 0);
            else if (target == typeof(Boolean))
                return BitConverter.ToBoolean(bytes, 0);
            else if (target == typeof(Char))
                return BitConverter.ToChar(bytes, 0);
            else if (target == typeof(String))
                return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            else if (target == typeof(IntPtr))
                return new IntPtr(BitConverter.ToInt64(bytes, 0));
            else if (target == typeof(byte))
                return bytes[0];
            else if (target == typeof(Int16[]))
                return new bool[bytes.Length / sizeof(Int16)].Select(x => BitConverter.ToInt16(bytes, i += 4)).ToArray();
            else if (target == typeof(Int32[]))
                return new bool[bytes.Length / sizeof(Int32)].Select(x => BitConverter.ToInt32(bytes, i += 4)).ToArray();
            else if (target == typeof(Int64[]))
                return new bool[bytes.Length / sizeof(Int64)].Select(x => BitConverter.ToInt64(bytes, i += 4)).ToArray();
            else if (target == typeof(UInt16[]))
                return new bool[bytes.Length / sizeof(UInt16)].Select(x => BitConverter.ToUInt16(bytes, i += 4)).ToArray();
            else if (target == typeof(UInt32[]))
                return new bool[bytes.Length / sizeof(UInt32)].Select(x => BitConverter.ToUInt32(bytes, i += 4)).ToArray();
            else if (target == typeof(UInt64[]))
                return new bool[bytes.Length / sizeof(UInt64)].Select(x => BitConverter.ToUInt64(bytes, i += 4)).ToArray();
            else if (target == typeof(Single[]))
                return new bool[bytes.Length / sizeof(Single)].Select(x => BitConverter.ToSingle(bytes, i += 4)).ToArray();
            else if (target == typeof(Double[]))
                return new bool[bytes.Length / sizeof(Double)].Select(x => BitConverter.ToDouble(bytes, i += 4)).ToArray();
            else if (target == typeof(Boolean[]))
                return new bool[bytes.Length / sizeof(Boolean)].Select(x => BitConverter.ToBoolean(bytes, i += 4)).ToArray();
            else if (target == typeof(Char[]))
                return new bool[bytes.Length / sizeof(Char)].Select(x => BitConverter.ToChar(bytes, i += 4)).ToArray();
            else if (target == typeof(IntPtr[]))
                return new bool[bytes.Length / sizeof(Int64)].Select(x => new IntPtr(BitConverter.ToInt64(bytes, i += 4))).ToArray();
            else if (target == typeof(byte[]))
                return bytes;
            else if (target == typeof(DateTime))
                return DateTime.FromBinary(BitConverter.ToInt64(bytes, 0));
            else if (target == typeof(TimeSpan))
                return TimeSpan.FromTicks(BitConverter.ToInt64(bytes, 0));
            else
                return this.Deserialize<T>(bytes);
        }

        /// <summary>
        /// Serialize an object of type T to an array of bytes.
        /// </summary>
        internal protected abstract byte[] Serialize<T>(T obj);

        /// <summary>
        /// Deserialize an array of bytes to an object of type T.
        /// </summary>
        internal protected abstract T Deserialize<T>(byte[] bytes);

        /// <summary>
        /// Deserialize an array of bytes to an object.
        /// </summary>
        internal protected virtual object Deserialize(byte[] bytes, Type target)
        {
            return this
                    .GetType()
                    .GetRuntimeMethods()
                    .First(x => x.Name == "Deserialize")
                    .MakeGenericMethod(new Type[] { target })
                    .Invoke(this, new object[] { bytes });
        }
        #endregion

        #region Members
        private SectorEnumerator Enumerator { get; set; }
        #endregion

        /// <summary>
        /// Empty constructor.
        /// Do not forget to call <see cref="Initialize"/>!
        /// </summary>
        protected CoreConnection()
        {

        }

        /// <summary>
        /// Call this method once <see cref="GetRWStream"/> is ready to provide a <see cref="Stream"/>.
        /// </summary>
        protected void Initialize()
        {
            Enumerator = new SectorEnumerator(RWStream, this);
        }

        /// <summary>
        /// Make a <see cref="Nucleus.Query{T}"/> to the database.
        /// A <see cref="Nucleus.Query{T}"/> implements <see cref="IList{T}"/> and is thread-safe.
        /// </summary>
        /// <param name="name">The name of the query. Cannot contain ';'.</param>
        public Query<T> Query<T>(string name)
        {
            return Query<T>(name, true);
        }

        /// <summary>
        /// Make a <see cref="Nucleus.DynamicDictionaryQuery"/> to the database.
        /// A <see cref="Nucleus.DynamicDictionaryQuery"/> implements <see cref="IEnumerable{T}"/> and is thread-safe.
        /// </summary>
        /// <param name="name">The name of the query. Cannot contain ';'.</param>
        public DynamicDictionaryQuery DictionaryQuery(string name)
        {
            return DictionaryQuery(name, true);
        }

        /// <summary>
        /// Make a <see cref="Nucleus.DictionaryQuery{T}"/> to the database.
        /// A <see cref="Nucleus.DictionaryQuery{T}"/> implements <see cref="IDictionary{string,T}"/> and is thread-safe.
        /// </summary>
        /// <param name="name">The name of the query. Cannot contain ';'.</param>
        public DictionaryQuery<T> DictionaryQuery<T>(string name)
        {
            return DictionaryQuery<T>(name, true);
        }

        /// <summary>
        /// Make a <see cref="Nucleus.Query{T}"/> to the database.
        /// A <see cref="Nucleus.Query{T}"/> implements <see cref="IList{T}"/> and is thread-safe.
        /// </summary>
        /// <param name="name">The name of the query. Cannot contain ';'.</param>
        public Query<T> Query<T>(string name, bool saveOnDisposed)
        {
            return new Query<T>(Enumerator.OfNameAndType(name, SectorType.Generic), saveOnDisposed);
        }

        /// <summary>
        /// Make a <see cref="Nucleus.DynamicDictionaryQuery"/> to the database.
        /// A <see cref="Nucleus.DynamicDictionaryQuery"/> implements <see cref="IEnumerable{T}"/> and is thread-safe.
        /// </summary>
        /// <param name="name">The name of the query. Cannot contain ';'.</param>
        public DynamicDictionaryQuery DictionaryQuery(string name, bool saveOnDisposed)
        {
            return new DynamicDictionaryQuery(Enumerator.OfNameAndType(name, SectorType.Dictionary), saveOnDisposed);
        }

        /// <summary>
        /// Make a <see cref="Nucleus.DictionaryQuery{T}"/> to the database.
        /// A <see cref="Nucleus.DictionaryQuery{T}"/> implements <see cref="IDictionary{string,T}"/> and is thread-safe.
        /// </summary>
        /// <param name="name">The name of the query. Cannot contain ';'.</param>
        public DictionaryQuery<T> DictionaryQuery<T>(string name, bool saveOnDisposed)
        {
            return new DictionaryQuery<T>(Enumerator.OfNameAndType(name, SectorType.Generic | SectorType.Dictionary), saveOnDisposed);
        }

        public void Dispose()
        {
            Enumerator.Dispose();
        }
    }
}
