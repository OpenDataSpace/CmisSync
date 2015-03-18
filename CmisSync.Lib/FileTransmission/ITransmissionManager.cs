
namespace CmisSync.Lib.FileTransmission {
    using System;

    /// <summary>
    /// Interface for a transmission manager. It is the main factory for new Transmission objects and the management interface for all running transmissions.
    /// </summary>
    public interface ITransmissionManager {
        /// <summary>
        /// Creates a new the transmission object and adds it to the manager. The manager decides when to and how the transmission gets removed from it.
        /// </summary>
        /// <returns>The transmission.</returns>
        /// <param name="type">Transmission type.</param>
        /// <param name="path">Full path.</param>
        /// <param name="cachePath">Cache path.</param>
        Transmission CreateTransmission(TransmissionType type, string path, string cachePath = null);
    }
}