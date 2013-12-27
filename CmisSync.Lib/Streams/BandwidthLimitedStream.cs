using System;
using System.IO;
using System.Diagnostics;

namespace CmisSync.Lib.Streams
{
    public class BandwidthLimitedStream : Stream
    {
        private object Lock = new object ();
        private long readLimit = -1;
        private Stopwatch ReadWatch;
        private Stopwatch WriteWatch;
        private Stream stream;
        public long ReadLimit {
            get { return this.readLimit;}
            set {
                if (value <= 0)
                    throw new ArgumentException ("Limit cannot be negative");
                lock (Lock) {
                    this.readLimit = value;
                }
            }
        }

        private long writeLimit = -1;

        public long WriteLimit {
            get { return this.writeLimit;}
            set {
                if (value <= 0)
                    throw new ArgumentException ("Limit cannot be negative");
                lock (Lock) {
                    this.writeLimit = value;
                }
            }
        }

        public BandwidthLimitedStream (Stream s)
        {
            init (s);
        }

        public BandwidthLimitedStream (Stream s, long limit)
        {
            init (s);
            ReadLimit = limit;
            WriteLimit = limit;
        }

        private void init(Stream s) {
            if (s == null)
                throw new ArgumentNullException ("Limited Stream is null, but must NOT be null");
            stream = s;
            WriteWatch = new Stopwatch();
            ReadWatch = new Stopwatch();
        }

        public void DisableLimits ()
        {
            lock (Lock) {
                this.readLimit = -1;
                this.writeLimit = -1;
            }
        }

        public void DisableReadLimit ()
        {
            lock (Lock) {
                this.readLimit = -1;
            }
        }

        public void DisableWriteLimit ()
        {
            lock (Lock) {
                this.writeLimit = -1;
            }
        }

        public override int Read (byte[] buffer, int offset, int count)
        {
            if(this.readLimit < 0)
                return stream.Read (buffer, offset, count);
            else{
                //TODO Sleep must be implemented
                return stream.Read (buffer, offset, count);
            }
        }

        public override void Write (byte[] buffer, int offset, int count)
        {
            if(this.WriteLimit < 0)
                stream.Write (buffer, offset, count);
            else{
                //TODO Sleep must be implemented
                stream.Write (buffer, offset, count);
            }
        }

        #region wrappedCalls
        public override bool CanRead {
            get {
                return this.stream.CanRead;
            }
        }

        public override bool CanSeek {
            get {
                return this.stream.CanSeek;
            }
        }

        public override bool CanWrite {
            get {
                return this.stream.CanWrite;
            }
        }

        public override long Length {
            get {
                return stream.Length;
            }
        }

        public override long Position {
            get {
                return stream.Position;
            }
            set {
                stream.Position = value;
            }
        }

        public override void Flush ()
        {
            stream.Flush ();
        }

        public override IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return stream.BeginRead (buffer, offset, count, callback, state);
        }

        public override long Seek (long offset, SeekOrigin origin)
        {
            return stream.Seek (offset, origin);
        }

        public override void SetLength (long value)
        {
            stream.SetLength (value);
        }

        protected override void Dispose (bool disposing)
        {
            base.Dispose (disposing);
        }
        #endregion
    }
}

