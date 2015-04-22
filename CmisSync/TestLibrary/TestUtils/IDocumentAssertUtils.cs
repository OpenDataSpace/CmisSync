//-----------------------------------------------------------------------
// <copyright file="IDocumentAssertUtils.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------

namespace TestLibrary.TestUtils {
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Client;

    using NUnit.Framework;

    public static class IDocumentAssertUtils {
        public static void AssertThatIfContentHashExistsItIsEqualTo(this IDocument doc, string content) {
            doc.AssertThatIfContentHashExistsItIsEqualToHash(ComputeSha1Hash(content));
        }

        public static void AssertThatIfContentHashExistsItIsEqualTo(this IDocument doc, byte[] content) {
            doc.AssertThatIfContentHashExistsItIsEqualToHash(ComputeSha1Hash(content));
        }

        public static void AssertThatIfContentHashExistsItIsEqualToHash(this IDocument doc, byte[] expectedHash, string type = "SHA-1") {
            Assert.That(doc.ContentStreamHash(type), Is.Null.Or.EqualTo(expectedHash));
        }

        public static bool VerifyThatIfTimeoutIsExceededContentHashIsEqualTo(this IDocument doc, string content, int timeoutInSeconds = 30) {
            int loops = 0;
            while (doc.ContentStreamHash() == null && loops < timeoutInSeconds) {
                loops++;
                Thread.Sleep(1000);
                doc.Refresh();
                doc.AssertThatIfContentHashExistsItIsEqualTo(content);
                if (doc.ContentStreamHash() != null) {
                    return true;
                }
            }

            return false;
        }

        public static byte[] ComputeSha1Hash(string content) {
            return ComputeSha1Hash(Encoding.UTF8.GetBytes(content));
        }

        public static byte[] ComputeSha1Hash(byte[] content) {
            using (var sha1 = SHA1.Create()) {
                return sha1.ComputeHash(content);
            }
        }
    }
}