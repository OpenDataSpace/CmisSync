//-----------------------------------------------------------------------
// <copyright file="CmisRepoCredentialsTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.ConfigTests {
    using System;

    using CmisSync.Lib.Config;

    using Moq;

    using NUnit.Framework;
    [TestFixture]
    public class CmisRepoCredentialsTest {
        [Test, Category("Fast")]
        public void DefaultConstructor() {
            var cred = new CmisRepoCredentials();
            Assert.IsNull(cred.Address);
            Assert.IsNull(cred.UserName);
            Assert.IsNull(cred.Password);
            Assert.IsNull(cred.RepoId);
        }

        [Test, Category("Fast")]
        public void SetRepoId() {
            string repoId = "RepoId";
            var cred = new CmisRepoCredentials() {
                RepoId = repoId
            };
            Assert.AreEqual(repoId, cred.RepoId);
        }
    }
}