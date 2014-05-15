namespace CmisSync.Lib.Events
{
    using System.IO;
    
    public interface IFSEvent : ISyncEvent
    {
        WatcherChangeTypes Type { get; }

        string Path { get; }

        bool IsDirectory();
    }
}

