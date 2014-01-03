using System;
using System.IO;

namespace CmisSync.Lib.Streams
{
    public class StreamWrapper : Stream
    {
        protected Stream Stream;
        public StreamWrapper (Stream stream)
        {
            if(stream == null)
                throw new ArgumentNullException("Given stream must not be null");
            this.Stream = stream;
        }

        #region wrapperCode
        public override bool CanRead {
            get {
                return Stream.CanRead;
            }
        }

        public override bool CanSeek {
            get {
                return Stream.CanSeek;
            }
        }

        public override bool CanWrite {
            get {
                return Stream.CanWrite;
            }
        }

        public override long Length {
            get {
                return Stream.Length;
            }
        }

        public override long Position {
            get {
                return Stream.Position;
            }
            set {
                Stream.Position = value;
            }
        }

        public override void Flush ()
        {
            Stream.Flush ();
        }

        public override IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return Stream.BeginRead (buffer, offset, count, callback, state);
        }

        public override long Seek (long offset, SeekOrigin origin)
        {
            return Stream.Seek (offset, origin);
        }

        public override int Read (byte[] buffer, int offset, int count)
        {
            return Stream.Read (buffer, offset, count);
        }

        public override void SetLength (long value)
        {
            Stream.SetLength (value);
        }

        public override void Write (byte[] buffer, int offset, int count)
        {
            Stream.Write (buffer, offset, count);
        }

        protected override void Dispose (bool disposing)
        {
            base.Dispose (disposing);
        }
        #endregion
    }
}

