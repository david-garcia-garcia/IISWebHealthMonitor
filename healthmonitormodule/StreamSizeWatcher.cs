using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;

namespace healthmonitormodule
{
    public class StreamSizeWatcher : Stream
    {
        private Stream _base;
        private long size = 0;

        public StreamSizeWatcher(Stream stream)
        {
            _base = stream;
        }

        public override void Flush()
        {
            _base.Flush();
        }

        public long getSize()
        {
            return this.size;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _base.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            size += buffer.Length;
            _base.Write(buffer, offset, count);
        }

        public override string ToString()
        {
            return _base.ToString();
        }

        #region Rest of the overrides
        public override bool CanRead
        {
            get { return _base.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _base.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _base.CanWrite; }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _base.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _base.SetLength(value);
        }

        public override long Length
        {
            get { return _base.Length; }
        }

        public override long Position
        {
            get
            {
                return _base.Position;
            }
            set
            {
                _base.Position = value;
            }
        }
        #endregion
    }
}