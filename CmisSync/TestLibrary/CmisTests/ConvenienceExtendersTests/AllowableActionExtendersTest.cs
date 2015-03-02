//-----------------------------------------------------------------------
// <copyright file="AllowableActionExtendersTest.cs" company="GRAU DATA AG">
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
﻿
namespace TestLibrary.CmisTests.ConvenienceExtendersTests {
    using System;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class AllowableActionExtendersTests {
        [Test, Category("Fast")]
        public void CanCreateDocument([Values(true, false)]bool readOnly) {
            var underTest = new Mock<IFolder>();
            underTest.SetupReadOnly(readOnly);
            Assert.That(underTest.Object.CanCreateDocument(), Is.EqualTo(!readOnly));
        }

        [Test, Category("Fast")]
        public void CanGetChildren([Values(true, false)]bool readOnly) {
            var underTest = new Mock<IFolder>();
            underTest.SetupReadOnly(readOnly);
            Assert.That(underTest.Object.CanGetChildren(), Is.True);
        }

        [Test, Category("Fast")]
        public void CanGetChildrenIfNoActionIsAvailable() {
            var underTest = new Mock<IFolder>();
            Assert.That(underTest.Object.CanGetChildren(), Is.Null);
        }

        [Test, Category("Fast")]
        public void CanDeleteObject([Values(true, false)]bool readOnly) {
            var underTest = new Mock<IDocument>();
            underTest.SetupReadOnly(readOnly);
            Assert.That(underTest.Object.CanDeleteObject(), Is.EqualTo(!readOnly));
        }

        [Test, Category("Fast")]
        public void CanGetFolderTree([Values(true, false)]bool readOnly, [Values(true, false)]bool supportsDescendants) {
            var underTest = new Mock<IFolder>();
            underTest.SetupReadOnly(readOnly, supportsDescendants);
            Assert.That(underTest.Object.CanGetDescendants(), Is.EqualTo(supportsDescendants));
            Assert.That(underTest.Object.CanGetFolderTree(), Is.EqualTo(supportsDescendants));
        }

        [Test, Category("Fast")]
        public void AreAllowableActionsAvailable(
            [Values(true, false)]bool includeActions,
            [Values(true, false)]bool includeAcls)
        {
            var underTest = new Mock<ISession>();
            var productName = "Generic Cmis server";
            var version = "1.0.0.0";
            var vendor = "Generic vendor";
            var repoInfo = underTest.SetupRepositoryInfo(productName, version, vendor);
            underTest.SetupDefaultOperationContext(includeAcls, includeActions);
            repoInfo.SetupCapabilities(acls: includeAcls ? DotCMIS.Enums.CapabilityAcl.Discover : DotCMIS.Enums.CapabilityAcl.None);

            Assert.That(underTest.Object.AreAllowableActionsAvailable(), Is.EqualTo(includeAcls || includeActions));
        }

        [Test, Category("Fast")]
        public void ImitateOldGdsCmisGw() {
            var underTest = new Mock<ISession>();
            underTest.SetupRepositoryInfo("GRAU DataSpace CMIS Gateway", "1.5.0", "GRAU DATA AG");
            underTest.SetupDefaultOperationContext(false, true);

            Assert.That(underTest.Object.AreAllowableActionsAvailable(), Is.False);
        }

        [Test, Category("Fast")]
        public void ImitateNewGdsCmisGw() {
            var underTest = new Mock<ISession>();
            underTest.SetupRepositoryInfo("GRAU DataSpace CMIS Gateway", "1.5.1120", "GRAU DATA AG");
            underTest.SetupDefaultOperationContext(false, true);

            Assert.That(underTest.Object.AreAllowableActionsAvailable(), Is.True);
        }
    }
}