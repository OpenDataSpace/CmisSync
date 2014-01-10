using System;
using System.IO;

namespace CmisSync.Lib.Events
{
    public class FSEvent : ISyncEvent
    {
        public WatcherChangeTypes Type { get; private set; }

        public string Path { get; private set; }

        public FSEvent (WatcherChangeTypes type, string path)
        {
            if (path == null) {
                throw new ArgumentNullException ("Argument null in FSEvent Constructor", "path");
            }
            Type = type;
            Path = path;
        }

        public override string ToString ()
        {
            return string.Format ("FSEvent with type \"{0}\" on path \"{1}\"", Type, Path);
        }

        public bool IsDirectory ()
        {
            //detect whether its a directory or file
            return (File.GetAttributes (Path) & FileAttributes.Directory) == FileAttributes.Directory;
        }
    }

    public class FSMovedEvent : FSEvent
    {
        public string OldPath { get; private set; }

        public FSMovedEvent (string oldPath, string newPath) : base(WatcherChangeTypes.Renamed, newPath)
        {
            OldPath = oldPath;
        }
    }
}

