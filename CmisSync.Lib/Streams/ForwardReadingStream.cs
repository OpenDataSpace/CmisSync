using System;
using System.IO;

namespace CmisSync.Lib.Streams
{
    public class ForwardReadingStream : StreamWrapper
    {
        private long pos = 0;
        public ForwardReadingStream (Stream nonSeekableStream) : base (nonSeekableStream)
        { }

        public override long Position {
            get {
                return pos;
            }
            set {
                base.Position = value;
                pos = value;
            }
        }

        public override int Read (byte[] buffer, int offset, int count)
        {
            int read = base.Read (buffer, offset, count);
            pos += read;
            return read;
        }
    }
}

