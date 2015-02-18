
namespace CmisSync.Lib.Queueing {
    using System;
    using System.Collections.Generic;

    public class Unsubscriber<T> : IDisposable {
        private List<IObserver<T>> observers;
        private IObserver<T> observer;

        public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer) {
            this.observers = observers;
            this.observer = observer;
        }

        public void Dispose() {
            if (this.observer != null && this.observers.Contains(this.observer)) {
                this.observers.Remove(this.observer);
            }
        }
    }
}