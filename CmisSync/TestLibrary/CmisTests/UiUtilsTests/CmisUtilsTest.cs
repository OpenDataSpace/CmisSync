//-----------------------------------------------------------------------
// <copyright file="CmisUtilsTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.CmisTests.UiUtilsTests {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using CmisSync.Lib;
    using CmisSync.Lib.Cmis.UiUtils;
    using CmisSync.Lib.Config;

    using DotCMIS;

    using NUnit.Framework;

    [TestFixture]
    public class CmisUtilsTest {
        [Test, Category("Fast")]
        public void BandwidthTest() {
            long bitPerSecond = 1;
            double bitPerSecondDouble = 1d;
            Assert.AreEqual(Utils.FormatBandwidth(bitPerSecond), Utils.FormatBandwidth(bitPerSecondDouble));
            Assert.True(Utils.FormatBandwidth(bitPerSecond).Contains("1 Bit/s"));
            bitPerSecond = 2;
            bitPerSecondDouble = bitPerSecond;
            Assert.AreEqual(Utils.FormatBandwidth(bitPerSecond), Utils.FormatBandwidth(bitPerSecondDouble));
            Assert.True(Utils.FormatBandwidth(bitPerSecond).Contains("2 Bit/s"));
            bitPerSecond = 1000;
            bitPerSecondDouble = bitPerSecond;
            Assert.AreEqual(Utils.FormatBandwidth(bitPerSecond), Utils.FormatBandwidth(bitPerSecondDouble));
            Assert.True(Utils.FormatBandwidth(bitPerSecond).Contains("1 KBit/s"), Utils.FormatBandwidth(bitPerSecond));
            bitPerSecond = 1100;
            Assert.True(Utils.FormatBandwidth(bitPerSecond).Replace(',', '.').Contains("1.1 KBit/s"), Utils.FormatBandwidth(bitPerSecond));
            bitPerSecond = 1499;
            Assert.True(Utils.FormatBandwidth(bitPerSecond).Replace(',', '.').Contains("1.5 KBit/s"), Utils.FormatBandwidth(bitPerSecond));
            bitPerSecond = 1500;
            Assert.True(Utils.FormatBandwidth(bitPerSecond).Replace(',', '.').Contains("1.5 KBit/s"), Utils.FormatBandwidth(bitPerSecond));
            bitPerSecond = 1000 * 1000;
            bitPerSecondDouble = bitPerSecond;
            Assert.AreEqual(Utils.FormatBandwidth(bitPerSecond), Utils.FormatBandwidth(bitPerSecondDouble));
            Assert.True(Utils.FormatBandwidth(bitPerSecond).Contains("1 MBit/s"), Utils.FormatBandwidth(bitPerSecond));
            bitPerSecond = 1000 * 1000 * 1000;
            bitPerSecondDouble = bitPerSecond;
            Assert.AreEqual(Utils.FormatBandwidth(bitPerSecond), Utils.FormatBandwidth(bitPerSecondDouble));
            Assert.True(Utils.FormatBandwidth(bitPerSecond).Contains("1 GBit/s"), Utils.FormatBandwidth(bitPerSecond));
            bitPerSecond = (1000 * 1000 * 1000) + (100 * 1000 * 1000);
            bitPerSecondDouble = bitPerSecond;
            Assert.AreEqual(Utils.FormatBandwidth(bitPerSecond), Utils.FormatBandwidth(bitPerSecondDouble));
            Assert.True(Utils.FormatBandwidth(bitPerSecond).Replace(',', '.').Contains("1.1 GBit/s"), Utils.FormatBandwidth(bitPerSecond));
        }

        [Test, Category("Fast")]
        public void FormatIntegerPercentTest() {
            int p = 5;
            Assert.AreEqual("5.0 %", Utils.FormatPercent(p).Replace(',', '.'));
        }

        [Test, Category("Fast")]
        public void FormatDoublePercentTest() {
            double p = 5.03;
            Assert.AreEqual("5.0 %", Utils.FormatPercent(p).Replace(',', '.'));
            p = 5.06;
            Assert.AreEqual("5.0 %", Utils.FormatPercent(p).Replace(',', '.'));
            p = 0.1;
            Assert.AreEqual("0.1 %", Utils.FormatPercent(p).Replace(',', '.'));
        }

        [Test, Category("Fast")]
        public void CreateUserAgent() {
            var useragent = Utils.CreateUserAgent();
            Assert.That(useragent.Contains(Backend.Version));
            Assert.That(useragent.Contains("hostname="));
            Assert.That(useragent.Contains(CultureInfo.CurrentCulture.Name));
        }

        [Test, Category("Fast")]
        public void CreateRegexFromIgnoreAllWildcard() {
            var regex = Utils.IgnoreLineToRegex("*");
            Assert.That(regex.IsMatch(string.Empty));
            Assert.That(regex.IsMatch(" "));
            Assert.That(regex.IsMatch("test"));
            Assert.That(regex.IsMatch("stuff.txt"));
        }

        [Test, Category("Fast")]
        public void CreateRegexFromIgnoreDotsAtTheBeginningWildcard() {
            var regex = Utils.IgnoreLineToRegex(".*");
            Assert.That(!regex.IsMatch(string.Empty));
            Assert.That(!regex.IsMatch("s."));
            Assert.That(!regex.IsMatch("test"));
            Assert.That(!regex.IsMatch("stuff.txt"));
            Assert.That(regex.IsMatch(".git"));
        }

        [Test, Category("Fast")]
        public void CreateRegexFromIgnoreTildeAtTheBeginningWildcard() {
            var regex = Utils.IgnoreLineToRegex("~*");
            Assert.That(regex.IsMatch("~test"));
            Assert.That(regex.IsMatch("~"));
            Assert.That(!regex.IsMatch("stuff.txt"));
            Assert.That(!regex.IsMatch(".~"));
        }

        [Test, Category("Fast")]
        public void CreateRegexFromIgnoreTempFilesWildcard() {
            var regex = Utils.IgnoreLineToRegex("*.tmp");
            Assert.That(regex.IsMatch("~test.tmp"));
            Assert.That(regex.IsMatch(".tmp"));
            Assert.That(!regex.IsMatch("stuff.tmp~"));
            Assert.That(!regex.IsMatch("tmp.test"));
        }

        [Test, Category("Fast")]
        public void IgnoreFolderByWildard() {
            var wildcards = new List<string>();
            wildcards.Add(".*");
            Assert.That(Utils.IsInvalidFolderName("test", wildcards), Is.False, "test is a valid folder name");
            Assert.That(Utils.IsInvalidFolderName(".test", wildcards), Is.True, ".test is not a valid folder name");
        }

        [Test, Category("Fast")]
        public void CreateMultipleServerCredentialsBasedOnTheGivenOne() {
            var userName = "User";
            var originalUrl = "https://demo.deutsche-wolke.de/wrongStuff";
            var password = new Password(Guid.NewGuid().ToString());
            var originalCredentials = new ServerCredentials {
                Address = new Uri(originalUrl),
                Password = password,
                Binding = BindingType.Browser,
                UserName = userName
            };

            var list = originalCredentials.CreateFuzzyCredentials();

            Assert.That(list, Is.Not.Null);
            Assert.That(list, Is.Not.Empty);
            Assert.That(list.First().Credentials, Is.EqualTo(originalCredentials));
            Assert.That(list[1].Credentials.Address.ToString(), Is.EqualTo(originalCredentials.Address.ToString()));
            Assert.That(list[1].Credentials.Binding, Is.Not.EqualTo(originalCredentials.Binding));
            foreach (var entry in list) {
                Assert.That(entry.Credentials.Password.ToString(), Is.EqualTo(password.ToString()));
                Assert.That(entry.Credentials.UserName, Is.EqualTo(userName));
                Console.WriteLine(entry);
            }
        }
    }
}