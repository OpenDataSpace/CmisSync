//-----------------------------------------------------------------------
// <copyright file="DBreezeTests.cs" company="GRAU DATA AG">
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

namespace TestLibrary.StorageTests.DataBaseTests {
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DBreeze;
    using DBreeze.DataTypes;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    public class DBreezeTests : IDisposable {
        private DBreezeEngine engine = null;
        private string path = null;
        private Mock<IFileInfo> file = null;

        [TestFixtureSetUp]
        public void InitCustomSerializator() {
            // Use Newtonsoft.Json as Serializator
            DBreeze.Utils.CustomSerializator.Serializator = JsonConvert.SerializeObject; 
            DBreeze.Utils.CustomSerializator.Deserializator = JsonConvert.DeserializeObject;
        }

        [SetUp]
        public void SetUp() {
            this.path = Path.Combine(Path.GetTempPath(), "DBreeze");
            this.engine = new DBreezeEngine(new DBreezeConfiguration { Storage = DBreezeConfiguration.eStorage.MEMORY });
            this.file = new Mock<IFileInfo>();
            this.file.SetupAllProperties();
            this.file.Setup(f => f.Length).Returns(1024);
            this.file.Setup(f => f.Name).Returns("FileTransmissionObjectsTest.file");
            this.file.Setup(f => f.FullName).Returns(Path.Combine(Path.GetTempPath(), this.file.Object.Name));
            this.file.Setup(f => f.Exists).Returns(true);
            this.file.Object.LastWriteTimeUtc = DateTime.UtcNow;
        }

        [TearDown]
        public void TearDown() {
            this.engine.Dispose();
            this.engine = null;
            if (Directory.Exists(this.path)) {
                Directory.Delete(this.path, true);
            }
        }

        [Test, Category("Fast"), Category("IT")]
        public void InsertInteger() {
            using (var tran = this.engine.GetTransaction()) {
                tran.Insert<int, int>("t1", 1, 2);
                tran.Commit();
                Assert.AreEqual(2, tran.Select<int, int>("t1", 1).Value);
            }
        }

        [Test, Category("Fast"), Category("IT")]
        public void InsertTestObject() {
            using (var tran = this.engine.GetTransaction()) {
                var folder = new TestClass {
                    Name = "Name"
                };
                tran.Insert<int, DbCustomSerializer<TestClass>>("objects", 1, folder);
                tran.Commit();
                Assert.AreEqual("Name", (tran.Select<int, DbCustomSerializer<TestClass>>("objects", 1).Value.Get as TestClass).Name);
            }
        }

        [Test, Category("Medium"), Category("IT")]
        public void CreateDbOnFsAndInsertAndSelectObject() {
            var conf = new DBreezeConfiguration {
                DBreezeDataFolderName = this.path,
                Storage = DBreezeConfiguration.eStorage.DISK
            };
            using (var engine = new DBreezeEngine(conf))
            using (var tran = engine.GetTransaction()) {
                var folder = new TestClass {
                    Name = "Name"
                };
                tran.Insert<int, DbCustomSerializer<TestClass>>("objects", 1, folder);
                tran.Commit();
                Assert.AreEqual("Name", (tran.Select<int, DbCustomSerializer<TestClass>>("objects", 1).Value.Get as TestClass).Name);
            }
        }

        [Test, Category("Fast"), Category("IT")]
        public void InsertAndSelectMappedObjectData() {
            using (var tran = this.engine.GetTransaction()) {
                string key = "key";
                string name = "name";
                var file = new MappedObject(name, key, MappedObjectType.File, null, null);
                tran.Insert<string, DbCustomSerializer<MappedObject>>("objects", key, file);
                Assert.That((tran.Select<string, DbCustomSerializer<MappedObject>>("objects", key).Value.Get as MappedObject).Equals(file));
            }
        }

        [Test, Category("Fast"), Category("IT")]
        public void InsertAndSelectFileTransmissionObjectData() {
            using (var tran = this.engine.GetTransaction()) {
                string key = "key";
                var remoteFile = new Mock<DotCMIS.Client.IDocument>();
                remoteFile.Setup(m => m.Id).Returns("RemoteObjectId");
                remoteFile.Setup(m => m.Paths).Returns(new List<string>() { "/RemoteFile" });
                var data = new FileTransmissionObject(CmisSync.Lib.Events.FileTransmissionType.UPLOAD_NEW_FILE, this.file.Object, remoteFile.Object);
                tran.Insert<string, DbCustomSerializer<FileTransmissionObject>>("objects", key, data);
                Assert.That((tran.Select<string, DbCustomSerializer<FileTransmissionObject>>("objects", key).Value.Get as FileTransmissionObject).Equals(data));
            }
        }

        #region builerplatecode
        public void Dispose() {
            if (this.engine != null) {
                this.engine.Dispose();
                this.engine = null;
            }
        }
        #endregion

        [Serializable]
        public class TestClass {
            public string Name { get; set; }
        }
    }
}