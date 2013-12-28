using System;
using System.IO;

namespace CmisSync.Lib.Streams
{
    public class OffsetStream : Stream
    {
        public long Offset{ get; private set; }
        private Stream stream;

        public OffsetStream (Stream stream, long offset = 0)
        {
            if(stream == null)
                throw new ArgumentNullException("Given stream must not be null");
            if(offset < 0)
                throw new ArgumentOutOfRangeException("A negative offset is forbidden");
            this.Offset = offset;
            this.stream = stream;
        }

        #region wrapperCode
        public override bool CanRead {
            get {
                return stream.CanRead;
            }
        }

        public override bool CanSeek {
            get {
                return stream.CanSeek;
            }
        }

        public override bool CanWrite {
            get {
                return stream.CanWrite;
            }
        }

        public override long Length {
            get {
                return this.stream.Length + Offset;
            }
        }

        public override long Position {
            get {
                return this.stream.Position + Offset;
            }
            set {
                if(value < Offset)
                    throw new ArgumentOutOfRangeException("given position is out of range");
                this.stream.Position = value - Offset;
            }
        }

        public override void Flush ()
        {
            this.stream.Flush ();
        }

        public override IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.stream.BeginRead (buffer, offset, count, callback, state);
        }

        public override long Seek (long offset, SeekOrigin origin)
        {
            return this.stream.Seek (offset, origin) + Offset;
        }

        public override int Read (byte[] buffer, int offset, int count)
        {
            return this.stream.Read (buffer, offset, count);
        }

        public override void SetLength (long value)
        {
            if(value < Offset)
                throw new ArgumentOutOfRangeException(String.Format("Given length {0} is smaller than Offset {1}", value, Offset));
            this.stream.SetLength (value - Offset);
        }

        public override void Write (byte[] buffer, int offset, int count)
        {
            this.stream.Write (buffer, offset, count);
        }
        #endregion
    }
}

