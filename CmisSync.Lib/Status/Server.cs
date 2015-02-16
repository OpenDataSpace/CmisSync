
namespace CmisSync.Lib.Status {
    using System;
    using System.IO;

    using NetMQ;
    public class Server {
        public Server() {
            using (NetMQContext ctx = NetMQContext.Create()) {
                using (var server = ctx.CreateResponseSocket()) {
                    var localSocketPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    server.Bind("ipc://" + localSocketPath);
                    using (var client = ctx.CreateRequestSocket()) {
                        client.Connect("ipc://" + localSocketPath);
                        client.Send("Hello");
                        Console.WriteLine("From Client: {0}", server.ReceiveString());
                        server.Send("Hi Back");
                        Console.WriteLine("From Server: {0}", client.ReceiveString());
                    }
                }
            }
        }
    }
}