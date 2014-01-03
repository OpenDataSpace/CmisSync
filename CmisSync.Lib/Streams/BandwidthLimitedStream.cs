using System;
using System.IO;
using System.Diagnostics;

namespace CmisSync.Lib.Streams
{
    public class BandwidthLimitedStream : StreamWrapper
    {
        private object Lock = new object ();
        private long readLimit = -1;
        private Stopwatch ReadWatch;
        private Stopwatch WriteWatch;
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

        public BandwidthLimitedStream (Stream s) : base(s)
        {
            init ();
        }

        public BandwidthLimitedStream (Stream s, long limit) : base(s)
        {
            init ();
            ReadLimit = limit;
            WriteLimit = limit;
        }

        private void init() {
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
                return Stream.Read (buffer, offset, count);
            else{
                //TODO Sleep must be implemented
                return Stream.Read (buffer, offset, count);
            }
        }

        public override void Write (byte[] buffer, int offset, int count)
        {
            if(this.WriteLimit < 0)
                Stream.Write (buffer, offset, count);
            else{
                //TODO Sleep must be implemented
                Stream.Write (buffer, offset, count);
            }
        }
    }
}

