
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

        public Array GetAllIcons(){
            return Enum.GetValues(typeof(Icons));
        }
    }
}