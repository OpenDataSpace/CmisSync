
namespace TestLibrary.MockedServer {
    using System;
    using DotCMIS;
    using DotCMIS.Client;

    public delegate void ContentChangeEventHandler(object sender, IChangeEvent e);

    public interface IContentChangeEventNotifier {
        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        event ContentChangeEventHandler ContentChanged;
    }
}