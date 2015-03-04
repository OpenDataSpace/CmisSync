//-----------------------------------------------------------------------
// <copyright file="UploadSlowWrittenFile.cs" company="GRAU DATA AG">
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

namespace TestLibrary.IntegrationTests {
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using DotCMIS.Client;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, Timeout(180000), TestName("UploadSlowlyWrittenFile")]
    public class UploadSlowWrittenFile : BaseFullRepoTest {
        [Test, Category("Slow")]
        public void SlowFileWriting(
            [Values(true, false)]bool contentChanges,
            [Values(FileAccess.ReadWrite, FileAccess.Write)]FileAccess access,
            [Values(FileShare.None, FileShare.ReadWrite, FileShare.Read, FileShare.Delete)]FileShare share)
        {
            int length = 10;
            this.ContentChangesActive = contentChanges;
            this.InitializeAndRunRepo(swallowExceptions: true);
            var file = new FileInfo(Path.Combine(this.localRootDir.FullName, "file"));
            using (var task = Task.Factory.StartNew(() => {
                using (var stream = file.Open(FileMode.CreateNew, access, share)) {
                    for (int i = 0; i < length; i++) {
                        Thread.Sleep(1000);
                        stream.WriteByte((byte)'0');
                    }
                }
            })) {
                while (!task.Wait(1000)) {
                    this.AddStartNextSyncEvent();
                    this.repo.Run();
                }

                this.AddStartNextSyncEvent();
                this.repo.Run();
            }

            Assert.That(this.localRootDir.GetFiles().Length, Is.EqualTo(1));
            Assert.That(this.remoteRootDir.GetChildren().TotalNumItems, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetFiles().First().Length, Is.EqualTo(length));
            Assert.That((this.remoteRootDir.GetChildren().First() as IDocument).ContentStreamLength, Is.EqualTo(length));
        }
    }
}