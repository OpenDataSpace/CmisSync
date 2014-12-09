
namespace TestLibrary.ConfigTests
{
    using System;

    using CmisSync.Lib.Config;

    using NUnit.Framework;

    [TestFixture]
    public class DefaultSettingsTests
    {
        [Test, Category("Fast")]
        public void GetInstance() {
            var config = DefaultEntries.Defaults;
            Assert.That(config, Is.Not.Null);
            Assert.That(Is.ReferenceEquals(config, DefaultEntries.Defaults));
        }
    }
}