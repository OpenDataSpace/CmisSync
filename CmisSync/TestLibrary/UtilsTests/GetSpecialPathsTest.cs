
namespace TestLibrary.UtilsTests {
    using System;

    using NUnit.Framework;

    [TestFixture, Category("Fast")]
    public class GetSpecialPathsTest {
        [Test, TestCaseSource("GetAllFolder")]
        public void SpecialFolder(Environment.SpecialFolder folder) {
            string path = Environment.GetFolderPath(folder, Environment.SpecialFolderOption.DoNotVerify);
            Assert.That(path, Is.Not.Null);
            Console.WriteLine(path);
        }

        public Array GetAllFolder(){
            return Enum.GetValues(typeof(Environment.SpecialFolder));
        }
    }
}