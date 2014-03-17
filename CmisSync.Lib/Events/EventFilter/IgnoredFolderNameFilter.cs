using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

namespace CmisSync.Lib.Events.Filter
{
    public class IgnoredFolderNameFilter : AbstractFileFilter
    {
        public IgnoredFolderNameFilter (ISyncEventQueue queue) : base (queue) { }
        private Object listLock = new Object();
        private List<Regex> wildcards = new List<Regex>();
        public List<string> Wildcards {
            set {
                if(value == null)
                    throw new ArgumentNullException("Given wildcards are null");
                lock(this.listLock)
                {
                    this.wildcards.Clear();
                    foreach(var wildcard in value)
                        this.wildcards.Add(Utils.IgnoreLineToRegex(wildcard));
                }
            }
        }
        public override bool Handle (ISyncEvent e)
        {
            if(e is FSEvent)
            {
                var fsEvent = e as FSEvent;
                string name;
                if(fsEvent.IsDirectory())
                {
                    name = Path.GetFileName(fsEvent.Path);
                }
                else
                {
                    name = Path.GetFileName(Path.GetDirectoryName(fsEvent.Path));
                }
                lock(listLock)
                {
                    foreach(var wildcard in wildcards)
                    {
                        if(wildcard.IsMatch(name))
                        {
                            Queue.AddEvent( new RequestIgnoredEvent(e, String.Format("Folder \"{0}\" matches regex {1}", fsEvent.Path, wildcard.ToString()), this));
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}

