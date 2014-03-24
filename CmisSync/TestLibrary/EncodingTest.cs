using System;
using CmisSync.Lib;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text;

namespace TestLibrary
{
    [TestFixture]
    public class EncodingTest
    {
        [Test, Category("Fast")]
        public void IsoEncodingTest()
        {
            Assert.IsTrue(CmisSync.Lib.Utils.IsValidISO88591("abcdefghijklmnopqrstuvwxyz"));
            Assert.IsTrue(CmisSync.Lib.Utils.IsValidISO88591("ABCDEFGHIJKLMNOPQRSTUVWXYZ"));
            Assert.IsTrue(CmisSync.Lib.Utils.IsValidISO88591("1234567890"));
            Assert.IsTrue(CmisSync.Lib.Utils.IsValidISO88591("ÄÖÜäöüß"));
            Assert.IsTrue(CmisSync.Lib.Utils.IsValidISO88591("-_.:,;#+*?!"));
            Assert.IsTrue(CmisSync.Lib.Utils.IsValidISO88591("/\\|¦<>§$%&()[]{}"));
            Assert.IsTrue(CmisSync.Lib.Utils.IsValidISO88591("'\"´`"));
            Assert.IsTrue(CmisSync.Lib.Utils.IsValidISO88591("@~¹²³±×"));
            Assert.IsTrue(CmisSync.Lib.Utils.IsValidISO88591("¡¢£¤¥¨©ª«¬®¯°µ¶·¸º»¼¼¾¿"));
            Assert.IsTrue(CmisSync.Lib.Utils.IsValidISO88591("ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝ"));
            Assert.IsTrue(CmisSync.Lib.Utils.IsValidISO88591("Þàáâãäåæçèéê"));
            Assert.IsFalse(CmisSync.Lib.Utils.IsValidISO88591("–€"));
        }

        [Test, Category("Fast")]
        public void ValidFileNameTest()
        {
            Assert.IsFalse(CmisSync.Lib.Utils.IsInvalidFileName("abcdefghijklmnopqrstuvwxyz"));
            Assert.IsFalse(CmisSync.Lib.Utils.IsInvalidFileName("ABCDEFGHIJKLMNOPQRSTUVWXYZ"));
            Assert.IsFalse(CmisSync.Lib.Utils.IsInvalidFileName("1234567890"));
            Assert.IsFalse(CmisSync.Lib.Utils.IsInvalidFileName("ÄÖÜäöüß"));
            Assert.IsFalse(CmisSync.Lib.Utils.IsInvalidFileName("-_.,;#+"));
            Assert.IsFalse(CmisSync.Lib.Utils.IsInvalidFileName("¦§$%&()[]{}"));
            Assert.IsFalse(CmisSync.Lib.Utils.IsInvalidFileName("'´`"));
            Assert.IsFalse(CmisSync.Lib.Utils.IsInvalidFileName("@~¹²³±×"));
            Assert.IsFalse(CmisSync.Lib.Utils.IsInvalidFileName("¡¢£¤¥¨©ª«¬®¯°µ¶·¸º»¼¼¾¿"));
            Assert.IsFalse(CmisSync.Lib.Utils.IsInvalidFileName("ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝ"));
            Assert.IsFalse(CmisSync.Lib.Utils.IsInvalidFileName("Þàáâãäåæçèéê"));
            Assert.IsTrue(CmisSync.Lib.Utils.IsInvalidFileName("?"), "?");
            Assert.IsTrue(CmisSync.Lib.Utils.IsInvalidFileName(":"), ":");
            Assert.IsTrue(CmisSync.Lib.Utils.IsInvalidFileName("/"), "/");
            Assert.IsTrue(CmisSync.Lib.Utils.IsInvalidFileName("\\"), "\\");
            Assert.IsTrue(CmisSync.Lib.Utils.IsInvalidFileName("\""), "\"");
            Assert.IsTrue(CmisSync.Lib.Utils.IsInvalidFileName("<"), "<");
            Assert.IsTrue(CmisSync.Lib.Utils.IsInvalidFileName(">"), ">");
            Assert.IsTrue(CmisSync.Lib.Utils.IsInvalidFileName("*"), "*");
            Assert.IsTrue(CmisSync.Lib.Utils.IsInvalidFileName("|"), "|");
            Assert.IsTrue(CmisSync.Lib.Utils.IsInvalidFileName("–€"), "Non Valid ISO 8859-1 Character accepted");
        }

        [Test, Category("Fast")]
        public void ValidFolderNameTest()
        {
            Assert.IsFalse(CmisSync.Lib.Utils.IsInvalidFolderName("abcdefghijklmnopqrstuvwxyz", new List<string>()));
            Assert.IsFalse(CmisSync.Lib.Utils.IsInvalidFolderName("ABCDEFGHIJKLMNOPQRSTUVWXYZ", new List<string>()));
            Assert.IsFalse(CmisSync.Lib.Utils.IsInvalidFolderName("1234567890", new List<string>()));
            Assert.IsFalse(CmisSync.Lib.Utils.IsInvalidFolderName("ÄÖÜäöüß", new List<string>()));
            Assert.IsFalse(CmisSync.Lib.Utils.IsInvalidFolderName("-_.,;#+", new List<string>()));
            Assert.IsFalse(CmisSync.Lib.Utils.IsInvalidFolderName("¦§$%&()[]{}", new List<string>()));
            Assert.IsFalse(CmisSync.Lib.Utils.IsInvalidFolderName("'´`", new List<string>()));
            Assert.IsFalse(CmisSync.Lib.Utils.IsInvalidFolderName("@~¹²³±×", new List<string>()));
            Assert.IsFalse(CmisSync.Lib.Utils.IsInvalidFolderName("¡¢£¤¥¨©ª«¬®¯°µ¶·¸º»¼¼¾¿", new List<string>()));
            Assert.IsFalse(CmisSync.Lib.Utils.IsInvalidFolderName("ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝ", new List<string>()));
            Assert.IsFalse(CmisSync.Lib.Utils.IsInvalidFolderName("Þàáâãäåæçèéê", new List<string>()));
            Assert.IsTrue(CmisSync.Lib.Utils.IsInvalidFolderName("?", new List<string>()), "?");
            Assert.IsTrue(CmisSync.Lib.Utils.IsInvalidFolderName(":", new List<string>()), ":");
            Assert.IsTrue(CmisSync.Lib.Utils.IsInvalidFolderName("/", new List<string>()), "/");
            Assert.IsTrue(CmisSync.Lib.Utils.IsInvalidFolderName("\\", new List<string>()), "\\");
            Assert.IsTrue(CmisSync.Lib.Utils.IsInvalidFolderName("\"", new List<string>()), "\"");
            Assert.IsTrue(CmisSync.Lib.Utils.IsInvalidFolderName("<", new List<string>()), "<");
            Assert.IsTrue(CmisSync.Lib.Utils.IsInvalidFolderName(">", new List<string>()), ">");
            Assert.IsTrue(CmisSync.Lib.Utils.IsInvalidFolderName("*", new List<string>()), "*");
            Assert.IsTrue(CmisSync.Lib.Utils.IsInvalidFolderName("|", new List<string>()), "|");
            Assert.IsTrue(CmisSync.Lib.Utils.IsInvalidFolderName("–€", new List<string>()), "Non Valid ISO 8859-1 Character accepted");
        }
    }
}

