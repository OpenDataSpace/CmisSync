//-----------------------------------------------------------------------
// <copyright file="MetaDataStorageTest.cs" company="GRAU DATA AG">
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
using System.IO;

namespace TestLibrary.StorageTests
{
    using System;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Storage;

    using DBreeze;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    public class MetaDataStorageTest
    {
        [TestFixtureSetUp]
        public void InitCustomSerializator()
        {
            // Use Newtonsoft.Json as Serializator
            DBreeze.Utils.CustomSerializator.Serializator = JsonConvert.SerializeObject; 
            DBreeze.Utils.CustomSerializator.Deserializator = JsonConvert.DeserializeObject;
        }

        private DBreezeEngine engine;
        private readonly IPathMatcher matcher = Mock.Of<IPathMatcher>();

        [SetUp]
        public void SetUp()
        {
            engine = new DBreezeEngine(new DBreezeConfiguration{ Storage = DBreezeConfiguration.eStorage.MEMORY });
        }

        [TearDown]
        public void TearDown()
        {
            engine.Dispose();
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsWithNoEngineAndNoMatcher()
        {
            new MetaDataStorage(null, null);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsWithNoEngine()
        {
            new MetaDataStorage(null, this.matcher);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsWithNoMatcher()
        {
            new MetaDataStorage(this.engine, null);
        }

        [Test, Category("Fast")]
        public void ConstructorTakesEngineWithoutFailure()
        {
            new MetaDataStorage(this.engine, this.matcher);
        }

        [Test, Category("Fast")]
        public void SetAndGetContentChangeToken()
        {
            string token = "token";
            var storage = new MetaDataStorage(this.engine, this.matcher);
            storage.ChangeLogToken = token;
            Assert.That(storage.ChangeLogToken, Is.EqualTo(token));
        }

        [Test, Category("Fast")]
        public void GetTokenFromEmptyStorageMustBeNull()
        {
            var storage = new MetaDataStorage(this.engine, this.matcher);
            Assert.That(storage.ChangeLogToken, Is.Null);
        }

        [Test, Category("Fast")]
        public void GetObjectByIdWithNotExistingIdMustReturnNull()
        {
            var storage = new MetaDataStorage(this.engine, this.matcher);
            Assert.That(storage.GetObjectByRemoteId("DOESNOTEXIST"), Is.Null);
        }

        [Test, Category("Fast")]
        public void GetObjectByPathWithNotExistingEntryMustReturnNull()
        {
            var storage = new MetaDataStorage(this.engine, this.matcher);
            var path = Mock.Of<IFileSystemInfo>( p =>
                                                p.FullName == Path.Combine(Path.GetTempPath(), "test"));
            Assert.That(storage.GetObjectByLocalPath(path), Is.Null);
        }
    }
}
