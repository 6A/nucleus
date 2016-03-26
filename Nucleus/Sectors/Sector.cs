using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nucleus
{
    [Flags]
    internal enum SectorType : byte
    {
        Generic = 1,
        Dynamic = 2,

        Dictionary = 4
    }

    internal class Sector : IDisposable
    {
        private const int BLOCK_SIZE = 1024 * 128;
        private bool isNew = false;

        /// <summary>
        /// Full length of Sector, excluding first four bytes
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Start of Sector
        /// </summary>
        public long Start { get { return Offset - 4; } }

        /// <summary>
        /// Start of Sector + four bytes
        /// </summary>
        public long Offset { get; set; }

        /// <summary>
        /// End of Sector / Start of next sector
        /// </summary>
        public long End { get { return Offset + Length; } }

        private volatile int connectedQueries = 0;
        internal int Connected { get { return connectedQueries; } set { connectedQueries = value; if (connectedQueries == 0 && ms != null) { ms.Dispose(); ms = null; } } }

        private MemoryStream ms = null;
        public MemoryStream Stream { get { if (ms == null) ms = enumerator.RequestMemoryStream(this); return ms; } }

        public SectorType Type { get; private set; }
        public OrderedDictionary Values { get; private set; }
        public string Name { get; private set; }
        public SectorEnumerator enumerator { get; set; }

        public event Action<object> Updated;

        protected internal byte[] Metadata
        {
            get
            {
                string values = Name + ';' + String.Join(";", Values.Select(x => x.Key.ToString() + ':' + x.Value.ToString()));
                int metadatalength = Encoding.UTF8.GetByteCount(values);
                byte[] metadata = new byte[5 + metadatalength];

                metadata[0] = (byte)Type;
                BitConverter.GetBytes(metadatalength).CopyTo(metadata, 1);
                Encoding.UTF8.GetBytes(values).CopyTo(metadata, 5);
                return metadata;
            }
        }

        public Sector(long end, string name, SectorType type, SectorEnumerator se)
        {
            Name = name;
            Type = type;
            Values = new OrderedDictionary();
            enumerator = se;
            
            Length = 0;
            Offset = end + 4;
            isNew = true;
            se.SectorMoved += UpdateSectorWhenMovement;
            se.SectorChanged += UpdateSectorWhenChange;
        }

        public Sector(long end, int length, byte[] bytes, SectorEnumerator se)
        {
            Length = length;
            Offset = end - length;
            enumerator = se;

            Type = (SectorType)bytes[0];

            int metadatalength = BitConverter.ToInt32(bytes, 1);
            string metadata = Encoding.UTF8.GetString(bytes, 5, metadatalength);
            Name = metadata.Substring(0, metadata.IndexOf(';'));
            metadata = metadata.Substring(Name.Length + 1);

            Values = new OrderedDictionary(metadata);
            se.SectorMoved += UpdateSectorWhenMovement;
            se.SectorChanged += UpdateSectorWhenChange;
        }

        public override bool Equals(object obj)
        {
            return obj is Sector && (obj as Sector).Type == this.Type && (obj as Sector).Name == this.Name;
        }

        public override int GetHashCode()
        {
            int i = 0;
            foreach (char c in Name.ToCharArray()) i *= (int)c / 10;
            return i * (int)Type;
        }

        private void UpdateSectorWhenMovement(Sector obj, long offset, int length)
        {
            if (obj != this && offset < this.Offset)
            {
                this.Offset -= length + offset - 4;
            }
        }

        private void UpdateSectorWhenChange(Sector obj, object o)
        {
            if (obj.Equals(this))
            {
                this.ms = obj.Stream;
                this.Values = obj.Values;
                this.connectedQueries = obj.connectedQueries;

                if (Updated != null)
                {
                    Updated(o);
                }
            }
        }

        public void Update()
        {
            int newLength = (int)Stream.Length;
            long oldOffset = Offset;
            byte[] metadata = Metadata;

            if (isNew)
            {
                enumerator.io.Seek(0, SeekOrigin.End);
            }
            else if (newLength + metadata.Length != Length) // overwrite old sector with what follows
            {
                byte[] bytes = new byte[BLOCK_SIZE];
                int read = 0;
                long left = enumerator.io.Length - Offset - Length;
                long total = 0;

                enumerator.io.Seek(End, SeekOrigin.Begin);
                do
                {
                    read = enumerator.io.Read(bytes, 0, BLOCK_SIZE > (int)left ? (int)left : BLOCK_SIZE);
                    enumerator.io.Seek(Start + total, SeekOrigin.Begin);
                    enumerator.io.Write(bytes, 0, read);
                    total += read;
                }
                while ((left -= read) > 0);

                enumerator.UpdateSector(this, oldOffset, Length);
            }
            else
            {
                enumerator.io.Seek(Start, SeekOrigin.Begin);
            }

            if (newLength == 0) // delete sector
            {
                if (isNew)
                {
                    return;
                }
                else
                {
                    if (Length > 0)
                    {
                        enumerator.io.SetLength(enumerator.io.Length - Length - 4);
                    }

                    isNew = true;
                    return;
                }
            }
            else // write sector to end
            {
                // append sector metadata
                enumerator.io.Write(BitConverter.GetBytes(newLength + metadata.Length), 0, 4);
                enumerator.io.Write(metadata, 0, metadata.Length);

                // append sector data
                Stream.WriteTo(enumerator.io);

                if (newLength + metadata.Length < Length) // resize if necessary
                {
                    enumerator.io.SetLength(enumerator.io.Length - Length + newLength + metadata.Length);
                }
                
                Length = newLength + metadata.Length;
                Offset = enumerator.io.Length - Length;
                isNew = false;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Stream.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Sector() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
