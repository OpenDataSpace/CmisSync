//-----------------------------------------------------------------------
// <copyright file="CRUDTests.cs" company="GRAU DATA AG">
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

namespace TestLibrary.IntegrationTests.ContentHashTests {
    using System;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Client;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, Timeout(360000), TestName("HashCRUD")]
    public class CRUDTests : BaseFullRepoTest {
        [Test, Category("Slow")]
        public void CreateDocAndImmediatelyDeleteContent() {
            this.EnsureThatContentHashesAreSupportedByServerTypeSystem();

            string content = "content";

            var doc = this.remoteRootDir.CreateDocument("file.txt", content);
            doc.DeleteContentStream(true);

            doc.AssertThatIfContentHashExistsItIsEqualTo(string.Empty);
        }

        [Test, Category("Slow")]
        public void CreateDocViaPwcAndDeleteContent() {
            this.EnsureThatContentHashesAreSupportedByServerTypeSystem();
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            string content = "content";

            var doc = this.remoteRootDir.CreateDocument("file.txt", content, checkedOut: true);
            var newId = doc.CheckIn(true, null, null, null);
            if (newId != null) {
                doc = this.session.GetObject(newId) as IDocument;
                doc.Refresh();
            }

            doc.DeleteContentStream(true);

            doc.AssertThatIfContentHashExistsItIsEqualTo(string.Empty);
        }
    }
}