//   CmisSync, a CMIS synchronization tool.
//   Copyright (C) 2012  Nicolas Raoul <nicolas.raoul@aegif.jp>
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;

using CmisSync.Lib.Events;

namespace CmisSync.Lib.Sync
{
    using log4net;

    public class CmisRepo : RepoBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CmisRepo));
        /// <summary>
        /// Remote folder to synchronize.
        /// </summary>
        private SynchronizedFolder synchronizedFolder;

        /// <summary>
        /// Track whether <c>Dispose</c> has been called.
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisRepo(RepoInfo repoInfo, IActivityListener activityListener)
            : base(repoInfo)
        {
            this.synchronizedFolder = new SynchronizedFolder(repoInfo, activityListener, this);
            this.Watcher.EnableEvents = true;
            Logger.Info(synchronizedFolder);
        }

        public override void Resume()
        {
            base.Resume();
        }

        /// <summary>
        /// Some file activity has been detected, add to queue
        /// </summary>
        public void OnFileActivity(object sender, FileSystemEventArgs args)
        {
            synchronizedFolder.Queue.AddEvent(new FSEvent(args.ChangeType, args.FullPath));
        }

        /// <summary>
        /// Dispose pattern implementation.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.synchronizedFolder.Dispose();
                }
                this.disposed = true;
            }
            base.Dispose(disposing);
        }

        public override double Size {get {throw new NotImplementedException();}}

    }
}
