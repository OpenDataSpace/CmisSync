//-----------------------------------------------------------------------
// <copyright file="EncodingTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.UtilsTests
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using CmisSync.Lib;

    using NUnit.Framework;

    [TestFixture]
    public class EncodingTest
    {
        [Test, Category("Fast")]
        public void IsoEncodingTest()
        {
            Assert.IsTrue(Utils.IsValidISO88591("abcdefghijklmnopqrstuvwxyz"));
            Assert.IsTrue(Utils.IsValidISO88591("ABCDEFGHIJKLMNOPQRSTUVWXYZ"));
            Assert.IsTrue(Utils.IsValidISO88591("1234567890"));
            Assert.IsTrue(Utils.IsValidISO88591("ÄÖÜäöüß"));
            Assert.IsTrue(Utils.IsValidISO88591("-_.:,;#+*?!"));
            Assert.IsTrue(Utils.IsValidISO88591("/\\|¦<>§$%&()[]{}"));
            Assert.IsTrue(Utils.IsValidISO88591("'\"´`"));
            Assert.IsTrue(Utils.IsValidISO88591("@~¹²³±×"));
            Assert.IsTrue(Utils.IsValidISO88591("¡¢£¤¥¨©ª«¬®¯°µ¶·¸º»¼¼¾¿"));
            Assert.IsTrue(Utils.IsValidISO88591("ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝ"));
            Assert.IsTrue(Utils.IsValidISO88591("Þàáâãäåæçèéê"));
            Assert.IsFalse(Utils.IsValidISO88591("–€"));
        }

        [Test, Category("Fast")]
        public void ValidFileNameTest()
        {
            Assert.IsFalse(Utils.IsInvalidFileName("abcdefghijklmnopqrstuvwxyz"));
            Assert.IsFalse(Utils.IsInvalidFileName("ABCDEFGHIJKLMNOPQRSTUVWXYZ"));
            Assert.IsFalse(Utils.IsInvalidFileName("1234567890"));
            Assert.IsFalse(Utils.IsInvalidFileName("ÄÖÜäöüß"));
            Assert.IsFalse(Utils.IsInvalidFileName("-_.,;#+"));
            Assert.IsFalse(Utils.IsInvalidFileName("¦§$%&()[]{}"));
            Assert.IsFalse(Utils.IsInvalidFileName("'´`"));
            Assert.IsFalse(Utils.IsInvalidFileName("@~¹²³±×"));
            Assert.IsFalse(Utils.IsInvalidFileName("¡¢£¤¥¨©ª«¬®¯°µ¶·¸º»¼¼¾¿"));
            Assert.IsFalse(Utils.IsInvalidFileName("ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝ"));
            Assert.IsFalse(Utils.IsInvalidFileName("Þàáâãäåæçèéê"));
            Assert.IsTrue(Utils.IsInvalidFileName("?"), "?");
            Assert.IsTrue(Utils.IsInvalidFileName(":"), ":");
            Assert.IsTrue(Utils.IsInvalidFileName("/"), "/");
            Assert.IsTrue(Utils.IsInvalidFileName("\\"), "\\");
            Assert.IsTrue(Utils.IsInvalidFileName("\""), "\"");
            Assert.IsTrue(Utils.IsInvalidFileName("<"), "<");
            Assert.IsTrue(Utils.IsInvalidFileName(">"), ">");
            Assert.IsTrue(Utils.IsInvalidFileName("*"), "*");
            Assert.IsTrue(Utils.IsInvalidFileName("|"), "|");
            Assert.IsTrue(Utils.IsInvalidFileName("–€"), "Non Valid ISO 8859-1 Character accepted");
        }

        [Test, Category("Fast")]
        public void ValidFolderNameTest()
        {
            Assert.IsFalse(Utils.IsInvalidFolderName("abcdefghijklmnopqrstuvwxyz", new List<string>()));
            Assert.IsFalse(Utils.IsInvalidFolderName("ABCDEFGHIJKLMNOPQRSTUVWXYZ", new List<string>()));
            Assert.IsFalse(Utils.IsInvalidFolderName("1234567890", new List<string>()));
            Assert.IsFalse(Utils.IsInvalidFolderName("ÄÖÜäöüß", new List<string>()));
            Assert.IsFalse(Utils.IsInvalidFolderName("-_.,;#+", new List<string>()));
            Assert.IsFalse(Utils.IsInvalidFolderName("¦§$%&()[]{}", new List<string>()));
            Assert.IsFalse(Utils.IsInvalidFolderName("'´`", new List<string>()));
            Assert.IsFalse(Utils.IsInvalidFolderName("@~¹²³±×", new List<string>()));
            Assert.IsFalse(Utils.IsInvalidFolderName("¡¢£¤¥¨©ª«¬®¯°µ¶·¸º»¼¼¾¿", new List<string>()));
            Assert.IsFalse(Utils.IsInvalidFolderName("ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝ", new List<string>()));
            Assert.IsFalse(Utils.IsInvalidFolderName("Þàáâãäåæçèéê", new List<string>()));
            Assert.IsTrue(Utils.IsInvalidFolderName("?", new List<string>()), "?");
            Assert.IsTrue(Utils.IsInvalidFolderName(":", new List<string>()), ":");
            Assert.IsTrue(Utils.IsInvalidFolderName("/", new List<string>()), "/");
            Assert.IsTrue(Utils.IsInvalidFolderName("\\", new List<string>()), "\\");
            Assert.IsTrue(Utils.IsInvalidFolderName("\"", new List<string>()), "\"");
            Assert.IsTrue(Utils.IsInvalidFolderName("<", new List<string>()), "<");
            Assert.IsTrue(Utils.IsInvalidFolderName(">", new List<string>()), ">");
            Assert.IsTrue(Utils.IsInvalidFolderName("*", new List<string>()), "*");
            Assert.IsTrue(Utils.IsInvalidFolderName("|", new List<string>()), "|");
            Assert.IsTrue(Utils.IsInvalidFolderName("–€", new List<string>()), "Non Valid ISO 8859-1 Character accepted");
        }

        [Test, Category("Fast")]
        public void IsStringNormalizedInFormD()
        {
            Assert.That(@"ä".IsNormalized(NormalizationForm.FormD));
            Assert.That(Utils.IsValidISO88591(@"ä"), Is.False);
        }
    }
}