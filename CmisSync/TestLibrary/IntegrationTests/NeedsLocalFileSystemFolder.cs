//-----------------------------------------------------------------------
// <copyright file="NeedsLocalFileSystemFolder.cs" company="GRAU DATA AG">
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

namespace TestLibrary.IntegrationTests
{
    using System;
    using System.IO;

    using TestLibrary.TestUtils;

    public abstract class NeedsLocalFileSystemFolder : IsTestWithConfiguredLog4Net
    {
        private static dynamic config;

        protected DirectoryInfo LocalTestDir { get; private set; }

        protected void TestFixtureSetUp() {
            config = ITUtils.GetConfig();
        }

        protected DirectoryInfo InitLocalTestDir() {
            if (config == null) {
                this.TestFixtureSetUp();
            }

            string subfolder = this.ToString() + "_" + Guid.NewGuid().ToString();
            this.LocalTestDir = new DirectoryInfo(Path.Combine(config[1].ToString(), subfolder));
            this.LocalTestDir.Create();
            return this.LocalTestDir;
        }

        protected void RemoveLocalTestDir() {
            if (this.LocalTestDir.Exists) {
                this.LocalTestDir.Delete(true);
            }
        }
    }
}