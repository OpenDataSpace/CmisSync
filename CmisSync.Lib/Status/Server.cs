
namespace CmisSync.Lib.Status {
    using System;
    using System.IO;

    using NetMQ;
    public class Server {
        public Server() {
            using (NetMQContext ctx = NetMQContext.Create()) {
                using (var server = ctx.CreateResponseSocket()) {
                    var localSocketPath = Path.GetTempFileName();
                    server.Bind("ipc://" + localSocketPath);

                    using (var client = ctx.CreateRequestSocket()) {
                        client.Connect("ipc://" + localSocketPath);
                        client.Send("Hello");

                        string m1 = server.ReceiveString();
                        Console.WriteLine("From Client: {0}", m1);
                        server.Send("Hi Back");

                        string m2 = client.ReceiveString();
                        Console.WriteLine("From Server: {0}", m2);
                    }
                }
            }
        }
    }
}