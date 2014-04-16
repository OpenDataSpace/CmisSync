
namespace TestLibrary.IntegrationTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.Data;

    using DBreeze;
    using DBreeze.DataTypes;

    using Newtonsoft.Json;

    using NUnit.Framework;

    public class DBreezeTests
    {
        private DBreezeEngine engine = null;
        private string path = null;

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
            this.path = Path.Combine(Path.GetTempPath(), "DBreeze");
            this.engine = new DBreezeEngine(new DBreezeConfiguration(){Storage = DBreezeConfiguration.eStorage.MEMORY});
        }

        [TearDown]
        public void TearDown()
        {
            this.engine.Dispose();
            if (Directory.Exists(this.path))
            {
                Directory.Delete(this.path, true);
            }
        }

        [Test, Category("Fast"), Category("IT")]
        public void InsertInteger()
        {
            using (var tran = this.engine.GetTransaction())
            {
                tran.Insert<int, int>("t1", 1, 2);
                tran.Commit();
                Assert.AreEqual(2, tran.Select<int, int>("t1", 1).Value);
            }
        }

        [Test, Category("Fast"), Category("IT")]
        public void InsertTestObject()
        {
            using (var tran = this.engine.GetTransaction())
            {
                var folder = new TestClass
                {
                    Name = "Name"
                };
                tran.Insert<int, DbCustomSerializer<TestClass>>("objects", 1, folder);
                tran.Commit();
                Assert.AreEqual("Name", (tran.Select<int, DbCustomSerializer<TestClass>>("objects", 1).Value.Get as TestClass).Name);
            }
        }

        [Test, Category("Medium"), Category("IT")]
        public void CreateDbOnFsAndInsertAndSelectObject()
        {
            var conf = new DBreezeConfiguration
            {
                DBreezeDataFolderName = this.path,
                Storage = DBreezeConfiguration.eStorage.DISK
            };
            using (var engine = new DBreezeEngine(conf))
            using (var tran = engine.GetTransaction())
            {
                var folder = new TestClass
                {
                    Name = "Name"
                };
                tran.Insert<int, DbCustomSerializer<TestClass>>("objects", 1, folder);
                tran.Commit();
                Assert.AreEqual("Name", (tran.Select<int, DbCustomSerializer<TestClass>>("objects", 1).Value.Get as TestClass).Name);
            }
        }

        [Serializable]
        public class TestClass
        {
            public string Name { get; set; }
        }
    }
}
