//-----------------------------------------------------------------------
// <copyright file="PersistentCookieStorageTest.cs" company="GRAU DATA AG">
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
                                                                         

namespace TestLibrary.StorageTests.DataBaseTests
{
    using System;
    using System.Net;

    using CmisSync.Lib.Storage;

    using DBreeze;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    [TestFixture]
    public class PersistentCookieStorageTest
    {
        private DBreezeEngine engine;

        [TestFixtureSetUp]
        public void InitCustomSerializator()
        {
            // Use Newtonsoft.Json as Serializator
            DBreeze.Utils.CustomSerializator.Serializator = JsonConvert.SerializeObject; 
            DBreeze.Utils.CustomSerializator.Deserializator = JsonConvert.DeserializeObject;
        }

        [SetUp]
        public void SetUp()
        {
            this.engine = new DBreezeEngine(new DBreezeConfiguration { Storage = DBreezeConfiguration.eStorage.MEMORY });
        }

        [TearDown]
        public void TearDown()
        {
            this.engine.Dispose();
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfDbEngineIsNull()
        {
            new PersistentCookieStorage(null);
        }

        [Test, Category("Fast")]
        public void GetCookieCollectionReturnEmptyCollectionOnNewDatabase()
        {
            var storage = new PersistentCookieStorage(this.engine);
            Assert.That(storage.Cookies, Is.Empty);
        }

        [Test, Category("Fast")]
        public void SetAndGetEmptyCookieCollection()
        {
            var storage = new PersistentCookieStorage(this.engine);

            storage.Cookies = new CookieCollection();

            Assert.That(storage.Cookies, Is.Empty);
        }

        [Test, Category("Fast")]
        public void SetCookieCollectionToNullCleansCollection()
        {
            var storage = new PersistentCookieStorage(this.engine);
            var collection = new CookieCollection();
            collection.Add(new Cookie {
                Name = "test",
                Expired = false,
                Expires = DateTime.Now.AddDays(1)
            });
            storage.Cookies = collection; 

            storage.Cookies = null;

            Assert.That(storage.Cookies, Is.Empty);
        }

        [Test, Category("Fast")]
        public void SetAndGetCookieCollectionsAreEqual()
        {
            var storage = new PersistentCookieStorage(this.engine);
            var collection = new CookieCollection();
            collection.Add(new Cookie {
                Name = "test",
                Expired = false,
                Expires = DateTime.Now.AddDays(1)
            });
            storage.Cookies = collection;

            Assert.That(storage.Cookies, Is.EqualTo(collection));
        }

        [Test, Category("Fast")]
        public void SaveCookieWithoutExpirationDate()
        {
            var storage = new PersistentCookieStorage(this.engine);
            var collection = new CookieCollection();
            collection.Add(new Cookie {
                Name = "JSESSIONID",
                Value = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
                Path = "/cmis",
                Expired = false
            });
            storage.Cookies = collection;

            Assert.That(storage.Cookies, Is.EqualTo(collection));
        }
    }
}