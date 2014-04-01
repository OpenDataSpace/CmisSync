#if __MonoCS__
using System;
using System.IO;
using CmisSync.Lib.Storage;

using NUnit.Framework;

using Moq;

namespace TestLibrary.StorageTests
{
    [TestFixture]
    public class ExtendedAttributeReaderUnixTest
    {
        private string path = "";

        [SetUp]
        public void SetUp()
        {
            path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        }

        [TearDown]
        public void CleanUp()
        {
            if(File.Exists(path))
                File.Delete(path);

            if(Directory.Exists(path))
                Directory.Delete(path);
        }

        [Test, Category("Fast")]
        public void DefaultConstructorWorks()
        {
            new ExtendedAttributeReaderUnix();
        }

        [Test, Category("Fast")]
        public void SettingPrefixOnConstructorDoesNotFails()
        {
            new ExtendedAttributeReaderUnix("system.");
        }

        [Test, Category("Medium")]
        public void GetNullAttributeFromNewFile()
        {
            using (File.Create(path));
            string key = "test";
            var reader = new ExtendedAttributeReaderUnix();
            Assert.That(reader.GetExtendedAttribute(path, key) == null);
        }

        [Test, Category("Medium")]
        public void SetAttributeToFile()
        {
            using (File.Create(path));
            string key = "test";
            string value = "value";
            var reader = new ExtendedAttributeReaderUnix();
            reader.SetExtendedAttribute(path, key, value);
            Assert.That(reader.GetExtendedAttribute(path, key).Equals(value));
        }

        [Test, Category("Medium")]
        public void OverwriteAttributeOnFile()
        {
            using (File.Create(path));
            string key = "test";
            string value = "value";
            string value2 = "value2";
            var reader = new ExtendedAttributeReaderUnix();
            reader.SetExtendedAttribute(path, key, value);
            reader.SetExtendedAttribute(path, key, value2);
            Assert.That(reader.GetExtendedAttribute(path, key).Equals(value2));
        }

        [Test, Category("Medium")]
        public void RemoveAttributeFromFile()
        {
            using (File.Create(path));
            string key = "test";
            string value = "value";
            var reader = new ExtendedAttributeReaderUnix();
            reader.SetExtendedAttribute(path, key, value);
            Assert.That(reader.GetExtendedAttribute(path, key).Equals(value));
            reader.RemoveExtendedAttribute(path, key);
            Assert.That(reader.GetExtendedAttribute(path, key) == null);
        }

        [Test, Category("Medium")]
        public void ListAttributesOfFile()
        {
            using (File.Create(path));
            string key = "test";
            string value = "value";
            var reader = new ExtendedAttributeReaderUnix();
            Assert.That(reader.ListAttributeKeys(path).Count == 0);
            reader.SetExtendedAttribute(path, key, value);
            Assert.That(reader.ListAttributeKeys(path).Count == 1);
            Assert.Contains("test", reader.ListAttributeKeys(path));
        }

        [Test, Category("Medium")]
        public void GetNullAttributeFromNewFolder()
        {
            Directory.CreateDirectory(path);
            string key = "test";
            var reader = new ExtendedAttributeReaderUnix();
            Assert.That(reader.GetExtendedAttribute(path, key) == null);
        }

        [Test, Category("Medium")]
        public void SetAttributeToFolder()
        {
            Directory.CreateDirectory(path);
            string key = "test";
            string value = "value";
            var reader = new ExtendedAttributeReaderUnix();
            reader.SetExtendedAttribute(path, key, value);
            Assert.That(reader.GetExtendedAttribute(path, key).Equals(value));
        }

        [Test, Category("Medium")]
        public void OverwriteAttributeOnFolder()
        {
            Directory.CreateDirectory(path);
            string key = "test";
            string value = "value";
            string value2 = "value2";
            var reader = new ExtendedAttributeReaderUnix();
            reader.SetExtendedAttribute(path, key, value);
            reader.SetExtendedAttribute(path, key, value2);
            Assert.That(reader.GetExtendedAttribute(path, key).Equals(value2));
        }

        [Test, Category("Medium")]
        public void RemoveAttributeFromFolder()
        {
            Directory.CreateDirectory(path);
            string key = "test";
            string value = "value";
            var reader = new ExtendedAttributeReaderUnix();
            reader.SetExtendedAttribute(path, key, value);
            Assert.That(reader.GetExtendedAttribute(path, key).Equals(value));
            reader.RemoveExtendedAttribute(path, key);
            Assert.That(reader.GetExtendedAttribute(path, key) == null);
        }

        [Test, Category("Medium")]
        public void ListAttributesOfFolder()
        {
            Directory.CreateDirectory(path);
            string key = "test";
            string value = "value";
            var reader = new ExtendedAttributeReaderUnix();
            Assert.That(reader.ListAttributeKeys(path).Count == 0);
            reader.SetExtendedAttribute(path, key, value);
            Assert.That(reader.ListAttributeKeys(path).Count == 1);
            Assert.Contains("test", reader.ListAttributeKeys(path));
        }
    }
}
#endif
