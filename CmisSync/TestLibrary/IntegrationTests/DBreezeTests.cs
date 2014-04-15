
using DBreeze.DataTypes;
using CmisSync.Lib.Data;
using Newtonsoft.Json;

namespace TestLibrary.IntegrationTests
{
    using System;
    using System.IO;

    using DBreeze;

    using NUnit.Framework;

    public class DBreezeTests
    {
        private DBreezeEngine engine = null;
        private string path = null;

        [SetUp]
        public void SetUp()
        {
            path = Path.Combine(Path.GetTempPath(), "DBreeze");
            engine = new DBreezeEngine(new DBreezeConfiguration(){Storage = DBreezeConfiguration.eStorage.MEMORY});
        }

        [TearDown]
        public void TearDown()
        {
            engine.Dispose();
            if(Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        [Test, Category("Fast"), Category("IT")]
        public void InsertInteger()
        {
            using (var tran = engine.GetTransaction())
            {
                tran.Insert<int, int>("t1", 1, 2);
                tran.Commit();
                Assert.AreEqual(2, tran.Select<int, int>("t1",1).Value);
            }
        }

        [Test, Category("Fast"), Category("IT")]
        public void InsertTestObject()
        {
            DBreeze.Utils.CustomSerializator.Serializator = JsonConvert.SerializeObject; 
            DBreeze.Utils.CustomSerializator.Deserializator = JsonConvert.DeserializeObject;
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

        [Test, Category("Medium"), Category("IT")]
        public void CreateDbOnFs()
        {
            var conf = new DBreezeConfiguration {
                DBreezeDataFolderName = path,
                Storage = DBreezeConfiguration.eStorage.DISK
            };
            using (var engine = new DBreezeEngine(conf))
            {

            }
        }

        [Serializable]
        public class TestClass
        {
            public string Name { get; set; }
        }
    }
}

