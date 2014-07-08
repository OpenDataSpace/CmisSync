//-----------------------------------------------------------------------
// <copyright file="ExtendedAttributeReaderDosTest.cs" company="GRAU DATA AG">
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
#if !__MonoCS__
namespace TestLibrary.StorageTests
{
    using System;
    using System.IO;
    using CmisSync.Lib.Storage;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.IntegrationTests;

    [TestFixture]
    public class ExtendedAttributeReaderDosTest
    {
        private string path = string.Empty;

        [SetUp]
        public void SetUp()
        {
            var config = ITUtils.GetConfig();
            string localPath = config[1].ToString();
            this.path = Path.Combine(localPath, Path.GetRandomFileName());
            var reader = new ExtendedAttributeReaderDos();
            if (!reader.IsFeatureAvailable(localPath)) {
                Assert.Ignore("Extended Attribute not available on this machine");
            }
        }

        [TearDown]
        public void CleanUp()
        {
            if (File.Exists(path)) {
                File.Delete(path);
            }

            if (Directory.Exists(path)) {
                Directory.Delete(path);
            }
        }

        [Test, Category("Fast")]
        public void DefaultConstructorWorks()
        {
            new ExtendedAttributeReaderDos();
        }

        [Test, Category("Medium")]
        [Category("ExtendedAttribute")]
        public void GetNullAttributeFromNewFile()
        {
            using (File.Create(path));
            string key = "test";
            var reader = new ExtendedAttributeReaderDos();
            Assert.That(reader.GetExtendedAttribute(path, key) == null);
        }

        [Test, Category("Medium")]
        [Category("ExtendedAttribute")]
        public void SetAttributeToFile()
        {
            using (File.Create(path));
            string key = "test";
            string value = "value";
            var reader = new ExtendedAttributeReaderDos();
            reader.SetExtendedAttribute(path, key, value);
            Assert.That(reader.GetExtendedAttribute(path, key).Equals(value));
        }

        [Test, Category("Medium")]
        [Category("ExtendedAttribute")]
        public void OverwriteAttributeOnFile()
        {
            using (File.Create(path));
            string key = "test";
            string value = "value";
            string value2 = "value2";
            var reader = new ExtendedAttributeReaderDos();
            reader.SetExtendedAttribute(path, key, value);
            reader.SetExtendedAttribute(path, key, value2);
            Assert.That(reader.GetExtendedAttribute(path, key).Equals(value2));
        }

        [Test, Category("Medium")]
        [Category("ExtendedAttribute")]
        public void RemoveAttributeFromFile()
        {
            using (File.Create(path));
            string key = "test";
            string value = "value";
            var reader = new ExtendedAttributeReaderDos();
            reader.SetExtendedAttribute(path, key, value);
            Assert.That(reader.GetExtendedAttribute(path, key).Equals(value));
            reader.RemoveExtendedAttribute(path, key);
            Assert.That(reader.GetExtendedAttribute(path, key) == null);
        }

        [Test, Category("Medium")]
        [Category("ExtendedAttribute")]
        public void ListAttributesOfFile()
        {
            using (File.Create(path));
            string key = "test";
            string value = "value";
            var reader = new ExtendedAttributeReaderDos();
            Assert.That(reader.ListAttributeKeys(path).Count == 0);
            reader.SetExtendedAttribute(path, key, value);
            Assert.That(reader.ListAttributeKeys(path).Count == 1);
            Assert.Contains("test", reader.ListAttributeKeys(path));
        }

        [Test, Category("Medium")]
        [Category("ExtendedAttribute")]
        public void GetNullAttributeFromNewFolder()
        {
            Directory.CreateDirectory(path);
            string key = "test";
            var reader = new ExtendedAttributeReaderDos();
            Assert.That(reader.GetExtendedAttribute(path, key) == null);
        }

        [Test, Category("Medium")]
        [Category("ExtendedAttribute")]
        public void SetAttributeToFolder()
        {
            Directory.CreateDirectory(path);
            string key = "test";
            string value = "value";
            var reader = new ExtendedAttributeReaderDos();
            reader.SetExtendedAttribute(path, key, value);
            Assert.That(reader.GetExtendedAttribute(path, key).Equals(value));
        }

        [Test, Category("Medium")]
        [Category("ExtendedAttribute")]
        public void OverwriteAttributeOnFolder()
        {
            Directory.CreateDirectory(path);
            string key = "test";
            string value = "value";
            string value2 = "value2";
            var reader = new ExtendedAttributeReaderDos();
            reader.SetExtendedAttribute(path, key, value);
            reader.SetExtendedAttribute(path, key, value2);
            Assert.That(reader.GetExtendedAttribute(path, key).Equals(value2));
        }

        [Test, Category("Medium")]
        [Category("ExtendedAttribute")]
        public void RemoveAttributeFromFolder()
        {
            Directory.CreateDirectory(path);
            string key = "test";
            string value = "value";
            var reader = new ExtendedAttributeReaderDos();
            reader.SetExtendedAttribute(path, key, value);
            Assert.That(reader.GetExtendedAttribute(path, key).Equals(value));
            reader.RemoveExtendedAttribute(path, key);
            Assert.That(reader.GetExtendedAttribute(path, key) == null);
        }

        [Test, Category("Medium")]
        [Category("ExtendedAttribute")]
        public void ListAttributesOfFolder()
        {
            Directory.CreateDirectory(path);
            string key = "test";
            string value = "value";
            var reader = new ExtendedAttributeReaderDos();
            Assert.That(reader.ListAttributeKeys(path).Count == 0);
            reader.SetExtendedAttribute(path, key, value);
            Assert.That(reader.ListAttributeKeys(path).Count == 1);
            Assert.Contains("test", reader.ListAttributeKeys(path));
        }

        [Test, Category("Medium")]
        [Category("ExtendedAttribute")]
        public void CheckAvailableOnPath()
        {
            var reader = new ExtendedAttributeReaderDos();
            reader.IsFeatureAvailable(Environment.CurrentDirectory);
        }

        [Test, Category("Medium")]
        [Category("ExtendedAttribute")]
        public void RemoveExtendedAttributeOfDirectoryByPassingNull()
        {
            Directory.CreateDirectory(path);
            string key = "test";
            var reader = new ExtendedAttributeReaderDos();
            reader.SetExtendedAttribute(path, key, null);
            Assert.That(reader.GetExtendedAttribute(path, key), Is.Null);
            Assert.That(reader.ListAttributeKeys(path).Count == 0);
        }

        [Test, Category("Medium"), Category("ExtendedAttribute")]
        public void RemoveExtendedAttributeOfFileByPassingNull()
        {
            using (File.Create(path));
            string key = "test";
            var reader = new ExtendedAttributeReaderDos();
            reader.SetExtendedAttribute(path, key, null);
            Assert.That(reader.GetExtendedAttribute(path, key), Is.Null);
            Assert.That(reader.ListAttributeKeys(path).Count == 0);
        }

        [Test, Category("Medium"), Category("ExtendedAttribute")]
        [ExpectedException(typeof(FileNotFoundException))]
        public void SetExtendedAttributeOnNonExistingFileThrowsIOException()
        {
            string key = "test";
            var reader = new ExtendedAttributeReaderDos();
            reader.SetExtendedAttribute(path, key, null);
        }

        [Test, Category("Medium"), Category("ExtendedAttribute")]
        [ExpectedException(typeof(FileNotFoundException))]
        public void GetExtendedAttributeOnNonExistingFileThrowsIOException()
        {
            string key = "test";
            var reader = new ExtendedAttributeReaderDos();
            reader.GetExtendedAttribute(path, key);
        }

        [Test, Category("Medium"), Category("ExtendedAttribute")]
        [ExpectedException(typeof(FileNotFoundException))]
        public void RemoveExtendedAttributeOnNonExistingFileThrowsIOException()
        {
            string key = "test";
            var reader = new ExtendedAttributeReaderDos();
            reader.RemoveExtendedAttribute(path, key);
        }
    }
}
#endif
