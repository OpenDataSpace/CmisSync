
namespace CmisSync.Lib.Streams {
    using System;
    using System.IO;

    public class AbortableStream : StreamWrapper {
        private bool aborted = false;
        private object l = new object();
        public AbortableStream(Stream s) : base(s) {
        }

        /// <summary>
        /// Read the specified buffer, offset and count.
        /// </summary>
        /// <param name='buffer'>
        /// Buffer.
        /// </param>
        /// <param name='offset'>
        /// Offset.
        /// </param>
        /// <param name='count'>
        /// Count.
        /// </param>
        public override int Read(byte[] buffer, int offset, int count) {
            lock(this.l) {
                if (this.aborted) {
                    throw new FileTransmission.AbortException();
                }
            }

            return this.Stream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Write the specified buffer, offset and count.
        /// </summary>
        /// <param name='buffer'>
        /// Buffer.
        /// </param>
        /// <param name='offset'>
        /// Offset.
        /// </param>
        /// <param name='count'>
        /// Count.
        /// </param>
        public override void Write(byte[] buffer, int offset, int count) {
            // for it may be chained before CryptoStream, we should write the content for CryptoStream has calculated the hash of the content
            this.Stream.Write(buffer, offset, count);

            lock(this.l) {
                if (this.aborted) {
                    throw new FileTransmission.AbortException();
                }
            }
        }

        /// <summary>
        /// Abort this instance.
        /// </summary>
        public void Abort() {
            lock(this.l) {
                this.aborted = true;
            }
        }
    }
}