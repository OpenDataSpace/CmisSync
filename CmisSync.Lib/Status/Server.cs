
namespace CmisSync.Lib.Status {
    using System;
    using System.IO;
    using System.Threading;

    using ZeroMQ;
    public class Server {
        public void Publish() {
            using (var ctx = new ZContext())
            using (var publisher = new ZSocket(ctx, ZSocketType.PUB)) {
                publisher.Bind("ipc://tmp");
                Console.WriteLine("Bound");
                Thread.Sleep(1000);
                for (int i = 0; i < 1000; i++) {
                    var msg = string.Format("bla {0}", i);
                    Console.WriteLine("Sending: " + msg);
                    publisher.Send(new ZFrame(msg));
                }
            }
        }
    }

    public class Subscriber {
        public void Subscribe() {
            using (var ctx = new ZContext())
            using (var subscriber = new ZSocket(ctx, ZSocketType.SUB)) {
                subscriber.Connect("ipc://tmp");
                subscriber.Subscribe(string.Empty);
                Console.WriteLine("Subscribed");
                for (int i = 0; i < 100; i++) {
                    using (ZFrame reply = subscriber.ReceiveFrame()) {
                        Console.WriteLine(" Received: {0}!", reply.ReadString());
                    }
                }
            }
        }
    }
}