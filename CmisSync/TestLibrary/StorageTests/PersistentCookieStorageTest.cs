namespace TestLibrary.StorageTests
{
    using System;
    using System.Net;

    using CmisSync.Lib.Storage;

    using DBreeze;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using Moq;

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
            this.engine = new DBreezeEngine(new DBreezeConfiguration{ Storage = DBreezeConfiguration.eStorage.MEMORY });
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
            collection.Add(new Cookie{
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
            collection.Add(new Cookie{
                Name = "test",
                Expired = false,
                Expires = DateTime.Now.AddDays(1)
            });
            storage.Cookies = collection; 

            Assert.That(storage.Cookies, Is.EqualTo(collection));
        }
    }
}

