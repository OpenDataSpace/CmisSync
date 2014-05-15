namespace CmisSync.Lib.Events
{
    using System.IO;
    
    public interface IFSMovedEvent : IFSEvent
    {
        string OldPath { get; }
    }
}

