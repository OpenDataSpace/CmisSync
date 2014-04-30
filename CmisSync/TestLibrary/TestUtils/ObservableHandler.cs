//-----------------------------------------------------------------------
// <copyright file="ObservableHandler.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------

namespace TestLibrary.TestUtils
{
    using System.Collections.Generic;

    using CmisSync.Lib.Events;

    using NUnit.Framework;

    public class ObservableHandler : SyncEventHandler {
        public List<ISyncEvent> list = new List<ISyncEvent>();

        public override int Priority
        {
            get { return int.MinValue; }
        }

        public override bool Handle(ISyncEvent e)
        {
            this.list.Add(e);
            return true;
        }

        public void AssertGotSingleFolderEvent(MetaDataChangeType metaType) {
            Assert.That(this.list.Count, Is.EqualTo(1));
            Assert.That(this.list[0], Is.TypeOf(typeof(FolderEvent)));
            var folderEvent = this.list[0] as FolderEvent;
            Assert.That(folderEvent.Remote, Is.EqualTo(metaType), "MetaDataChangeType incorrect");
        }

        public void AssertGotSingleFileEvent(MetaDataChangeType metaType, ContentChangeType contentType) {
            Assert.That(this.list.Count, Is.EqualTo(1));
            Assert.That(this.list[0], Is.TypeOf(typeof(FileEvent)));
            var fileEvent = this.list[0] as FileEvent;

            Assert.That(fileEvent.Remote, Is.EqualTo(metaType), "MetaDataChangeType incorrect");
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(contentType), "ContentChangeType incorrect");
        }
    }
}