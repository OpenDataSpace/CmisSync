
namespace CmisSync.Lib.Status {
    using System;
    using System.IO;

    using NetMQ;
    public class Server {
        public void Publish() {
            using (var ctx = NetMQContext.Create()) {
                using (var publisher = ctx.CreatePublisherSocket()) {
                    publisher.Bind("ipc://tmp");
                    for (int i = 0; i < 1000; i++) {
                        var msg = string.Format("bla {0}", i);
                        Console.WriteLine("Sending: " + msg);
                        publisher.Send(msg);
                    }
                }
            }
        }
    }

    public class Subscriber {
        public void Subscribe() {
            using (var ctx = NetMQContext.Create()) {
                using (var subscriber = ctx.CreateSubscriberSocket()) {
                    subscriber.Connect("ipc://tmp");
                    subscriber.Subscribe(string.Empty);
                    for (int i = 0; i < 100; i++) {
                        //Console.WriteLine(subscriber.ReceiveString());
                    }
                }
            }
        }
    }
}