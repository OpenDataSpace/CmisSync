//-----------------------------------------------------------------------
// <copyright file="DefaultSettingsTests.cs" company="GRAU DATA AG">
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

namespace TestLibrary.ConfigTests {
    using System;

    using CmisSync.Lib.Config;

    using NUnit.Framework;

    [TestFixture]
    public class DefaultSettingsTests {
        [Test, Category("Fast")]
        public void GetInstance() {
            var config = DefaultEntries.Defaults;
            Assert.That(config, Is.Not.Null);
            Assert.That(Is.ReferenceEquals(config, DefaultEntries.Defaults));
        }

        [Test, Category("Fast")]
        public void GetName() {
            var config = DefaultEntries.Defaults;
            Assert.That(config.Name, Is.Not.Null);
        }

        [Test, Category("Fast")]
        public void GetUrl() {
            Assert.That(DefaultEntries.Defaults.Url, Is.Not.Null);
        }

        [Test, Category("Fast")]
        public void GetBinding() {
            var binding = DefaultEntries.Defaults.Binding;
            Assert.That(binding, Is.Null.Or.EqualTo(DotCMIS.BindingType.AtomPub).Or.EqualTo(DotCMIS.BindingType.Browser));
        }
    }
}