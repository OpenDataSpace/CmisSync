
namespace TestLibrary.UtilsTests
{
    using System;

    using CmisSync.Lib;

    using NUnit.Framework;

    [TestFixture]
    public class BackendTest
    {
        [Test, Category("Fast")]
        public void BackendCompileTimeTest()
        {
            var date = Backend.RetrieveLinkerTimestamp;
            Assert.That(date, Is.Not.Null);
            Assert.That(date, Is.EqualTo(Backend.RetrieveLinkerTimestamp));
        }
    }
}