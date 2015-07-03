
namespace CmisSync.Lib.FileTransmission {
    using System;

    public interface ITransmissionAggregator {
        void Add(Transmission transmission);
    }
}