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

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Client;

    using NUnit.Framework;

    public static class IDocumentAssertUtils {
        public static void AssertThatIfContentHashExistsItIsEqualTo(this IDocument doc, string content) {
            doc.AssertThatIfContentHashExistsItIsEqualTo(Encoding.UTF8.GetBytes(content));
        }

        public static void AssertThatIfContentHashExistsItIsEqualTo(this IDocument doc, byte[] content) {
            doc.AssertThatIfContentHashExistsItIsEqualToHash(SHA1.Create().ComputeHash(content));
        }

        public static void AssertThatIfContentHashExistsItIsEqualToHash(this IDocument doc, byte[] expectedHash, string type = "SHA-1") {
            Assert.That(doc.ContentStreamHash(type), Is.Null.Or.EqualTo(expectedHash));
        }
    }
}