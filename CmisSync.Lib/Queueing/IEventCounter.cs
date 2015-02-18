
namespace CmisSync.Lib.Queueing {
    using System;

    using CmisSync.Lib.Events;

    public interface IEventCounter: IDisposable {
        void Increase(ICountableEvent e);
        void Decrease(ICountableEvent e);
    }
}