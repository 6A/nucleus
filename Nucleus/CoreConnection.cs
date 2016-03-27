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
        /// Serialize an object of type T to an array of bytes.
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

        /// <summary>
        /// Empty constructor
        /// </summary>
        protected CoreConnection()
        {

        }

        /// <summary>
        /// Call this method once <see cref="GetRWStream"/> is ready to provide a <see cref="Stream"/>.
        /// </summary>
        protected void Initialize()
        {
            Enumerator = new SectorEnumerator(GetRWStream(), this);
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
