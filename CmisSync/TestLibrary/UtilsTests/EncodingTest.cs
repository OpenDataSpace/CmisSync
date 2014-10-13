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
        private readonly string validChars = @"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890ÄÖÜäöüß-_.:,;#+*?!/\|<>§$%&()[]{}`'@~¹²³±×¡¢£¥©ª«¬®¯°µ¶·º»¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞàáâãäåæçèéê€–¦´¤¨¸¼¼¾";
        private readonly string invalidFolderNameChars = "?:/\\\"<>|*";
        private readonly string invalidFileNameChars = "?:/\\\"<>|*";

        [Test, Category("Fast")]
        public void ValidFileNameTest()
        {
            foreach (char c in this.invalidFileNameChars.ToCharArray()) {
                Assert.That(Utils.IsInvalidFileName(c.ToString()), Is.True, c.ToString());
            }

            foreach (char c in this.validChars.ToCharArray()) {
                if (!this.invalidFileNameChars.Contains(c.ToString())) {
                    Assert.That(Utils.IsInvalidFileName(c.ToString()), Is.False, c.ToString());
                }
            }
        }

        [Test, Category("Fast")]
        public void ValidFolderNameTest()
        {
            foreach (char c in this.invalidFolderNameChars.ToCharArray()) {
                Assert.That(Utils.IsInvalidFolderName(c.ToString()), Is.True, c.ToString());
            }

            foreach (char c in this.validChars.ToCharArray()) {
                if (!this.invalidFolderNameChars.Contains(c.ToString())) {
                    Assert.That(Utils.IsInvalidFolderName(c.ToString()), Is.False, c.ToString());
                }
            }
        }

        [Test, Category("Fast")]
        public void IsStringNormalizedInFormD()
        {
            Assert.That(@"ä".IsNormalized(NormalizationForm.FormD));
        }
    }
}