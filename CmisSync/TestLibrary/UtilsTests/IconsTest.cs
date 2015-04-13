//-----------------------------------------------------------------------
// <copyright file="IconsTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.UtilsTests {
    using System;

    using CmisSync.Lib.UiUtils;

    using NUnit.Framework;

    [TestFixture]
    public class IconsTest {
        [Test, Category("Fast"), TestCaseSource("GetAllIcons")]
        public void AllIconNamesAreNotEmpty(Icons icon) {
            Assert.That(icon.GetName(), Is.Null.Or.Not.EqualTo(string.Empty));
        }

        [Test, Category("Fast"), TestCaseSource("GetAllIcons")]
        public void GetIconWithExtensionAddsTypeToName(Icons icon) {
            Assert.That(icon.GetNameWithTypeExtension(), Is.Null.Or.Contains("."));
        }

        [Test, Category("Fast")]
        public void PlatformIsCorrect() {
            PlatformID expectedPlatform;
#if __COCOA__
            expectedPlatform = PlatformID.MacOSX;
#elif __MonoCS__
            expectedPlatform = PlatformID.Unix;
#else
            expectedPlatform = PlatformID.Win32NT;
#endif
            Assert.That(Environment.OSVersion.Platform, Is.EqualTo(expectedPlatform));
        }

        [Test, Category("Fast"), TestCaseSource("GetAllIcons")]
        public void GetIconWithSizeAndExtensionOnLinux(Icons icon) {
            PlatformID expectedPlatform = Environment.OSVersion.Platform;
#if !__COCOA__ && __MonoCS__
            if (icon.GetName() != null) {
                Assert.That(icon.GetNameWithSizeAndTypeExtension(), Is.StringContaining(icon.GetName()));
                Assert.That(icon.GetNameWithSizeAndTypeExtension(), Is.Not.EqualTo(icon.GetNameWithTypeExtension()));
            } else {
                Assert.That(icon.GetNameWithSizeAndTypeExtension(), Is.Null);
            }
#else
            Assert.That(icon.GetNameWithSizeAndTypeExtension(), Is.EqualTo(icon.GetNameWithTypeExtension()));
#endif
        }

        public Array GetAllIcons() {
            return Enum.GetValues(typeof(Icons));
        }
    }
}