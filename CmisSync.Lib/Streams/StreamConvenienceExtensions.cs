
namespace CmisSync.Lib.Streams {
    using System;
    using System.IO;

    public static class StreamConvenienceExtensions {
        public static void CopyTo(this Stream input, Stream output, int bufferSize, int bytes) {
            byte[] buffer = new byte[bufferSize];
            int read;
            while (bytes > 0 && (read = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0) {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }
    }
}