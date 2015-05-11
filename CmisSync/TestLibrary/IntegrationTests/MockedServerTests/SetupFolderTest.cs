//-----------------------------------------------------------------------
// <copyright file="SetupFolderTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.IntegrationTests.MockedServerTests {
    using System;

    using DotCMIS.Client;
    using DotCMIS.Enums;

    using MockedServer;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class SetupFolderTest {
        [Test, Category("Fast")]
        public void Constructor([Values(true, false)]bool withParent, [Values(null, "folderId")]string id) {
            string name = "folder";
            IFolder parent = withParent ? Mock.Of<IFolder>() : null;
            var underTest = new MockedFolder(name, id, parent);

            Assert.That(underTest.Object.BaseTypeId, Is.EqualTo(BaseTypeId.CmisFolder));
            Assert.That(underTest.Object.ObjectType.Id, Is.EqualTo(BaseTypeId.CmisFolder.GetCmisValue()));
            Assert.That(underTest.Object.ChangeToken, Is.Not.Null);
        }
    }
}