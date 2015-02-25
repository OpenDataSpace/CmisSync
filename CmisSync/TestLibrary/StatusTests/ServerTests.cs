
namespace TestLibrary.StatusTests {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using CmisSync.Lib.Status;

    using NUnit.Framework;
    [TestFixture]
    public class ServerTests {
        [Test, Category("Fast")]
        public void RunServer() {
            Task.Factory.StartNew(() => {
                var server = new Server();
                Thread.Sleep(1000);
                server.Publish();
            });
            var sub = new Subscriber();
            sub.Subscribe();
        }
    }
}