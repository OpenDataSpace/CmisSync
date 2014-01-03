using System;
using System.IO;

namespace CmisSync.Lib.Streams
{
    public class OffsetStream : StreamWrapper
    {
        public long Offset{ get; private set; }

        public OffsetStream (Stream stream, long offset = 0) : base(stream)
        {
            if(offset < 0)
                throw new ArgumentOutOfRangeException("A negative offset is forbidden");
            this.Offset = offset;
        }

        #region overrideCode
        public override long Length {
            get {
                return Stream.Length + Offset;
            }
        }

        public override long Position {
            get {
                return Stream.Position + Offset;
            }
            set {
                if(value < Offset)
                    throw new ArgumentOutOfRangeException("given position is out of range");
                Stream.Position = value - Offset;
            }
        }

        public override long Seek (long offset, SeekOrigin origin)
        {
            return Stream.Seek (offset, origin) + Offset;
        }

        public override void SetLength (long value)
        {
            if(value < Offset)
                throw new ArgumentOutOfRangeException(String.Format("Given length {0} is smaller than Offset {1}", value, Offset));
            Stream.SetLength (value - Offset);
        }
        #endregion
    }
}

