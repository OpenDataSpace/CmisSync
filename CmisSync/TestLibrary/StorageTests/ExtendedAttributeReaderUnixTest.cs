//-----------------------------------------------------------------------
// <copyright file="ExtendedAttributeReaderUnixTest.cs" company="GRAU DATA AG">
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
#if __MonoCS__
using System;
using System.IO;
using CmisSync.Lib.Storage;

using NUnit.Framework;

using Moq;

using TestLibrary.IntegrationTests;

namespace TestLibrary.StorageTests
{
    [TestFixture]
    public class ExtendedAttributeReaderUnixTest
    {
        private string path = string.Empty;

        [SetUp]
        public void SetUp()
        {
            var config = ITUtils.GetConfig();
            string localPath = config[1].ToString();
            path = Path.Combine(localPath, Path.GetRandomFileName());
            var reader = new ExtendedAttributeReaderUnix();
            if(!reader.IsFeatureAvailable(localPath)) {
                Assert.Ignore("Extended Attribute not available on this machine");
            }
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

        [Test, Category("Medium"), Category("ExtendedAttribute")]
        public void GetNullAttributeFromNewFile()
        {
            using (File.Create(path));
            string key = "test";
            var reader = new ExtendedAttributeReaderUnix();
            Assert.That(reader.GetExtendedAttribute(path, key), Is.Null);
        }

        [Test, Category("Medium"), Category("ExtendedAttribute")]
        public void SetAttributeToFile()
        {
            using (File.Create(path));
            string key = "test";
            string value = "value";
            var reader = new ExtendedAttributeReaderUnix();
            reader.SetExtendedAttribute(path, key, value);
            Assert.That(reader.GetExtendedAttribute(path, key).Equals(value));
        }

        [Test, Category("Medium"), Category("ExtendedAttribute")]
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

        [Test, Category("Medium"), Category("ExtendedAttribute")]
        public void RemoveAttributeFromFile()
        {
            using (File.Create(path));
            string key = "test";
            string value = "value";
            var reader = new ExtendedAttributeReaderUnix();
            reader.SetExtendedAttribute(path, key, value);
            Assert.That(reader.GetExtendedAttribute(path, key).Equals(value));
            reader.RemoveExtendedAttribute(path, key);
            Assert.That(reader.GetExtendedAttribute(path, key), Is.Null);
        }

        [Test, Category("Medium"), Category("ExtendedAttribute")]
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

        [Test, Category("Medium"), Category("ExtendedAttribute")]
        public void GetNullAttributeFromNewFolder()
        {
            Directory.CreateDirectory(path);
            string key = "test";
            var reader = new ExtendedAttributeReaderUnix();
            Assert.That(reader.GetExtendedAttribute(path, key), Is.Null);
        }

        [Test, Category("Medium"), Category("ExtendedAttribute")]
        public void SetAttributeToFolder()
        {
            Directory.CreateDirectory(path);
            string key = "test";
            string value = "value";
            var reader = new ExtendedAttributeReaderUnix();
            reader.SetExtendedAttribute(path, key, value);
            Assert.That(reader.GetExtendedAttribute(path, key).Equals(value));
        }

        [Test, Category("Medium"), Category("ExtendedAttribute")]
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

        [Test, Category("Medium"), Category("ExtendedAttribute")]
        public void RemoveAttributeFromFolder()
        {
            Directory.CreateDirectory(path);
            string key = "test";
            string value = "value";
            var reader = new ExtendedAttributeReaderUnix();
            reader.SetExtendedAttribute(path, key, value);
            Assert.That(reader.GetExtendedAttribute(path, key).Equals(value));
            reader.RemoveExtendedAttribute(path, key);
            Assert.That(reader.GetExtendedAttribute(path, key), Is.Null);
        }

        [Test, Category("Medium"), Category("ExtendedAttribute")]
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

        [Test, Category("Medium"), Category("ExtendedAttribute")]
        public void CheckAvailableOnPath()
        {
            var reader = new ExtendedAttributeReaderUnix();
            reader.IsFeatureAvailable(Environment.CurrentDirectory);
        }

        [Test, Category("Medium"), Category("ExtendedAttribute")]
        public void RemoveNonExistingAttributeFromFile()
        {
            using (File.Create(path));
            string key = "test";
            var reader = new ExtendedAttributeReaderUnix();
            reader.RemoveExtendedAttribute(path, key);
            Assert.That(reader.GetExtendedAttribute(path, key), Is.Null);
        }

        [Test, Category("Medium"), Category("ExtendedAttribute")]
        public void RemoveNonExistingAttributeFromFolder()
        {
            Directory.CreateDirectory(path);
            string key = "test";
            var reader = new ExtendedAttributeReaderUnix();
            reader.RemoveExtendedAttribute(path, key);
            Assert.That(reader.GetExtendedAttribute(path, key), Is.Null);
        }

        //WARNING: do not use Expected Exceptions for these tests
        //as there is a bug where ExtendedAttributeException matches FileNotFoundException
        [Test, Category("Medium"), Category("ExtendedAttribute")]
        public void SetExtendedAttributeOnNonExistingFileThrowsFileNotFoundException()
        {
            string key = "test";
            var reader = new ExtendedAttributeReaderUnix();
            try{
                reader.SetExtendedAttribute(path, key, null);
            } catch (FileNotFoundException) {
                return;
            }
            Assert.Fail("FileNotFoundException not thrown");
        }

        [Test, Category("Medium"), Category("ExtendedAttribute")]
        public void GetExtendedAttributeOnNonExistingFileThrowsFileNotFoundException()
        {
            string key = "test";
            var reader = new ExtendedAttributeReaderUnix();
            try{
                reader.GetExtendedAttribute(path, key);
            } catch (FileNotFoundException) {
                return;
            }
            Assert.Fail("FileNotFoundException not thrown");
        }

        [Test, Category("Medium"), Category("ExtendedAttribute")]
        public void RemoveExtendedAttributeOnNonExistingFileThrowsFileNotFoundException()
        {
            string key = "test";
            var reader = new ExtendedAttributeReaderUnix();
            try{
                reader.RemoveExtendedAttribute(path, key);
            } catch (FileNotFoundException) {
                return;
            }
            Assert.Fail("FileNotFoundException not thrown");
        }
    }
}
#endif
