using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        protected abstract Stream GetRWStream();

        /// <summary>
        /// Serialize an object of type T to an array of bytes for saving.
        /// </summary>
        internal protected abstract byte[] Serialize<T>(T obj);

        /// <summary>
        /// Deserialize an array of bytes to an object of type T.
        /// </summary>
        internal protected abstract T Deserialize<T>(byte[] bytes);
        #endregion

        #region Members
        private SectorEnumerator Enumerator { get; set; }
        #endregion

        protected CoreConnection()
        {

        }

        protected void Initialize()
        {
            Enumerator = new SectorEnumerator(GetRWStream(), this);
        }

        public Query<T> Query<T>(string name)
        {
            return Query<T>(name, true);
        }

        public DynamicDictionaryQuery DictionaryQuery(string name)
        {
            return DictionaryQuery(name, true);
        }

        public DictionaryQuery<T> DictionaryQuery<T>(string name)
        {
            return DictionaryQuery<T>(name, true);
        }

        public Query<T> Query<T>(string name, bool saveOnDisposed)
        {
            return new Query<T>(Enumerator.OfNameAndType(name, SectorType.Generic), saveOnDisposed);
        }

        public DynamicDictionaryQuery DictionaryQuery(string name, bool saveOnDisposed)
        {
            return new DynamicDictionaryQuery(Enumerator.OfNameAndType(name, SectorType.Generic), saveOnDisposed);
        }

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
