using CmisSync.Lib.Events;

using System.Collections.Generic;
using NUnit.Framework;

namespace TestLibrary.TestUtils
{
    public class ObservableHandler : SyncEventHandler {
        public List<ISyncEvent> list = new List<ISyncEvent>();

        public override bool Handle(ISyncEvent e)
        {
            list.Add(e);
            return true;
        }

        public override int Priority
        {
            get {return 1;}
        }

        public void AssertGotSingleFolderEvent(MetaDataChangeType metaType) {
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0], Is.TypeOf(typeof(FolderEvent)));
            var folderEvent = list[0] as FolderEvent;
            Assert.That(folderEvent.Remote, Is.EqualTo(metaType), "MetaDataChangeType incorrect");
            
        }

        public void AssertGotSingleFileEvent(MetaDataChangeType metaType, ContentChangeType contentType) {
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0], Is.TypeOf(typeof(FileEvent)));
            var fileEvent = list[0] as FileEvent;

            Assert.That(fileEvent.Remote, Is.EqualTo(metaType), "MetaDataChangeType incorrect");
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(contentType), "ContentChangeType incorrect");
            
        }
    }
}
