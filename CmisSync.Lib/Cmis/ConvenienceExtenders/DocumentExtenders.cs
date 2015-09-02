
namespace CmisSync.Lib.Cmis.ConvenienceExtenders {
    using System;
    using System.IO;
    using System.Text;

    using DotCMIS.Client;
    using DotCMIS.Data.Impl;

    /// <summary>
    /// DotCMIS Document extenders.
    /// </summary>
    public static class DocumentExtenders {
        /// <summary>
        /// Returns the hash of the content stream on the server.
        /// </summary>
        /// <returns>The hash.</returns>
        /// <param name="doc">Document with the content stream.</param>
        /// <param name="type">Type of the requested hash.</param>
        public static byte[] ContentStreamHash(this IDocument doc, string type = "SHA-1") {
            if (doc == null) {
                throw new ArgumentNullException("doc");
            }

            if (type == null) {
                throw new ArgumentNullException("type");
            }

            if (doc.Properties == null) {
                return null;
            }

            string prefix = string.Format("{{{0}}}", type.ToLower());
            foreach (var prop in doc.Properties) {
                if (prop.Id == "cmis:contentStreamHash") {
                    if (prop.Values != null) {
                        foreach (string entry in prop.Values) {
                            if (entry.StartsWith(prefix)) {
                                return StringToByteArray(entry.Substring(prefix.Length));
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the content stream of the document.
        /// </summary>
        /// <returns>The content.</returns>
        /// <param name="doc">Remote document.</param>
        /// <param name="content">New content as string.</param>
        /// <param name="overwrite">If set to <c>true</c> overwrites existing content.</param>
        /// <param name="refresh">If set to <c>true</c> refreshs the original remote doc instance.</param>
        public static IObjectId SetContent(this IDocument doc, string content, bool overwrite = true, bool refresh = true) {
            if (doc == null) {
                throw new ArgumentNullException("doc");
            }

            byte[] c = Encoding.UTF8.GetBytes(content);
            var contentStream = new ContentStream() {
                FileName = doc.Name,
                MimeType = MimeType.GetMIMEType(doc.Name),
                Length = c.LongLength
            };
            using (var stream = new MemoryStream(c)) {
                contentStream.Stream = stream;
                return doc.SetContentStream(contentStream, overwrite, refresh);
            }
        }

        /// <summary>
        /// Appends the content to the given doc.
        /// </summary>
        /// <returns>The resulting doc.</returns>
        /// <param name="doc">Cmis Document.</param>
        /// <param name="content">Content to be appended.</param>
        /// <param name="lastChunk">If set to <c>true</c>, the flag for the last chunk is set.</param>
        public static IDocument AppendContent(this IDocument doc, string content, bool lastChunk = true) {
            if (doc == null) {
                throw new ArgumentNullException("doc");
            }

            byte[] c = Encoding.UTF8.GetBytes(content);
            var contentStream = new ContentStream() {
                FileName = doc.Name,
                MimeType = MimeType.GetMIMEType(doc.Name),
                Length = c.LongLength
            };
            using (var stream = new MemoryStream(c)) {
                contentStream.Stream = stream;
                return doc.AppendContentStream(contentStream, lastChunk);
            }
        }

        /// <summary>
        /// Hex string to byte array.
        /// </summary>
        /// <returns>The byte array.</returns>
        /// <param name="hex">Hex string without leading 0x.</param>
        private static byte[] StringToByteArray(string hex) {
            if (hex == null) {
                throw new ArgumentNullException("hex");
            }

            if ((hex.Length & 1) == 1) {
                throw new ArgumentException("The binary key cannot have an odd number of digits");
            }

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < (hex.Length >> 1); ++i) {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + GetHexVal(hex[(i << 1) + 1]));
            }

            return arr;
        }

        private static int GetHexVal(char hex) {
            int val = (int)hex;
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
    }
}