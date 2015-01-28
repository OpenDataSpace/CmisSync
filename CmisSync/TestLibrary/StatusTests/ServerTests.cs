
namespace TestLibrary.StatusTests {
    using System;

    using CmisSync.Lib.Status;

    using NUnit.Framework;
    [TestFixture]
    public class ServerTests {
        [Test, Category("Fast")]
        public void RunServer() {
            new Server();
        }
    }
}