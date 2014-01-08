using System;
using System.Timers;

namespace CmisSync.Lib.Events
{
    public class SyncScheduler : SyncEventHandler, IDisposable
    {
        public static readonly int POLLSCHEDULERPRIORITY = 1000;
        private double interval;
        private SyncEventQueue queue;
        private Timer timer;
        public SyncScheduler (SyncEventQueue queue, double pollInterval = 5000)
        {
            if(queue == null)
                throw new ArgumentNullException("Given queue must not be null");
            if(pollInterval <= 0)
                throw new ArgumentException("pollinterval must be greater than zero");

            this.interval = pollInterval;
            this.queue = queue;
            this.timer = new Timer(this.interval);
            this.timer.Elapsed += delegate(object sender, ElapsedEventArgs e) {
                this.queue.AddEvent(new StartNextSyncEvent());
            };
            this.Start();
        }

        public void Start() {
            this.timer.Start();
        }

        public void Stop() {
            this.timer.Stop();
        }

        public override bool Handle (ISyncEvent e)
        {
            RepoConfigChangedEvent config = e as RepoConfigChangedEvent;
            if(config!=null)
            {
                double newInterval = config.RepoInfo.PollInterval;
                if( newInterval> 0 && this.interval != newInterval)
                {
                    this.interval = newInterval;
                    Stop ();
                    timer.Interval = this.interval;
                    Start ();
                }
                return false;
            }
            StartNextSyncEvent start = e as StartNextSyncEvent;
            if(start != null && start.FullSyncRequested)
            {
                Stop ();
                Start();
            }
            return false;
        }

        public override int Priority {
            get {
                return POLLSCHEDULERPRIORITY;
            }
        }

        public void Dispose() {
            timer.Stop();
            timer.Dispose();
        }
    }
}

