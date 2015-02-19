
namespace TestLibrary.UtilsTests {
    using System;

    using CmisSync.Lib;

    using NUnit.Framework;

    [TestFixture]
    public class NameOfPropertyTest {
        [Test, Category("Fast")]
        public void GetPropertyNameOfAnInstance() {
            var testClass = new TestClass();
            Assert.That(Utils.NameOf(() => testClass.TestProperty), Is.EqualTo("TestProperty"));
        }

        [Test, Category("Fast")]
        public void GetProperyNameOfByPassingAClass() {
            Assert.That(Utils.NameOf((TestClass t) => t.TestProperty), Is.EqualTo("TestProperty"));
        }

        private class TestClass {
            public string TestProperty { get; set; }
        }
    }
}