using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nucleus
{
    internal class SectorEnumerator : IDisposable
    {
        public Stream io;
        public CoreConnection core;
        private List<Sector> cache;

        public event Action<Sector, long, int> SectorMoved;
        public event Action<Sector, object> SectorChanged;

        public SectorEnumerator(Stream stream, CoreConnection coreco)
        {
            io = stream;
            cache = Read().OrderBy(x => x.Offset).ToList();
            core = coreco;
        }

        public void RemoveSector(Sector s)
        {
            cache.Remove(s);
        }

        public void UpdateSector(Sector s, long offset, int length)
        {
            cache.Remove(s);
            cache.Add(s);

            if (SectorMoved != null)
            {
                SectorMoved(s, offset, length);
            }
        }

        public void UpdateSector(Sector s, object o)
        {
            cache.Remove(s);
            cache.Add(s);

            if (SectorChanged != null)
            {
                SectorChanged(s, o);
            }
        }

        public bool TryReadBytes(long offset, int length, out byte[] bytes)
        {
            io.Seek(offset, SeekOrigin.Begin);

            byte[] buffer = new byte[length];

            if (io.Read(buffer, 0, length) == length)
            {
                bytes = buffer;
                return true;
            }
            else
            {
                bytes = buffer;
                return false;
            }
        }

        public Sector OfNameAndType(string name, SectorType type)
        {
            name = name.ToLower();

            if (name.ToCharArray().Contains(';'))
            {
                throw new FormatException("Illegal character in sector name: ';'");
            }

            Sector sector = cache.FirstOrDefault(x => x.Name == name && x.Type == type);

            if (sector != null)
            {
                return sector;
            }
            else
            {
                sector = new Sector(io.Length, name, type, this);
                cache.Add(sector);
                return sector;
            }
        }

        public MemoryStream RequestMemoryStream(Sector s)
        {
            MemoryStream ms = new MemoryStream();
            var metadata = s.Metadata;
            byte[] bytes = new byte[s.Length == 0 ? 0 : s.Length - metadata.Length];

            io.Seek((int)s.Offset + metadata.Length, SeekOrigin.Begin);
            io.Read(bytes, 0, bytes.Length);
            ms.Write(bytes, 0, bytes.Length);

            bytes = null;
            metadata = null;
            return ms;
        }

        private IEnumerable<Sector> Read()
        {
            int read;
            byte[] bytes = new byte[4];

            while ((read = io.Read(bytes, 0, 4)) == 4)
            {
                int length = BitConverter.ToInt32(bytes, 0);
                bytes = new byte[length];

                if (length > 0 && (read = io.Read(bytes, 0, length)) == length)
                {
                    yield return new Sector(io.Position, length, bytes, this);
                }
                else
                {
                    throw new EndOfStreamException("Unexpected end of stream");
                }
            }
        }



        public override string ToString()
        {
            long originalPos = io.Position;
            io.Seek(0, SeekOrigin.Begin);
            StringBuilder sb = new StringBuilder();
            int read;
            byte[] bytes = new byte[4096];
            while ((read = io.Read(bytes, 0, 4096)) > 0)
                sb.Append(Encoding.UTF8.GetString(bytes, 0, read));
            io.Seek(originalPos, SeekOrigin.Begin);
            return sb.ToString();
        }

        public void Dispose()
        {
            cache = null;
        }
    }
}
