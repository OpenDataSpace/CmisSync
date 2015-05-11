
namespace TestLibrary.StatusTests {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using CmisSync.Lib.Status;

    using NUnit.Framework;
    [TestFixture]
    public class ServerTests {
        [Test, Category("Fast"), Timeout(2000)]
        public void RunServer() {
            Task.Factory.StartNew(() => {
                var server = new Server();
                Console.WriteLine("Publishing");
                server.Publish();
            });
            var sub = new Subscriber();
            sub.Subscribe();
        }
    }
}