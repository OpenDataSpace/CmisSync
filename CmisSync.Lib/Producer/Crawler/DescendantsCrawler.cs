//-----------------------------------------------------------------------
// <copyright file="DescendantsCrawler.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Producer.Crawler
{
    using System;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Filter;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using log4net;

    /// <summary>
    /// Decendants crawler.
    /// </summary>
    public class DescendantsCrawler : ReportingSyncEventHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DescendantsCrawler));
        private IActivityListener activityListener;
        private IDescendantsTreeBuilder treebuilder;
        private CrawlEventGenerator eventGenerator;
        private CrawlEventNotifier notifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="DescendantsCrawler"/> class.
        /// </summary>
        /// <param name="queue">Sync Event Queue.</param>
        /// <param name="remoteFolder">Remote folder.</param>
        /// <param name="localFolder">Local folder.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="filter">Aggregated filter.</param>
        /// <param name="activityListener">Activity listner.</param>
        public DescendantsCrawler(
            ISyncEventQueue queue,
            IFolder remoteFolder,
            IDirectoryInfo localFolder,
            IMetaDataStorage storage,
            IFilterAggregator filter,
            IActivityListener activityListener)
            : base(queue)
        {
            if (remoteFolder == null) {
                throw new ArgumentNullException("Given remoteFolder is null");
            }

            if (localFolder == null) {
                throw new ArgumentNullException("Given localFolder is null");
            }

            if (storage == null) {
                throw new ArgumentNullException("Given storage is null");
            }

            if (filter == null) {
                throw new ArgumentNullException("Given filter is null");
            }

            if (activityListener == null) {
                throw new ArgumentNullException("Given activityListener is null");
            }

            this.activityListener = activityListener;
            this.treebuilder = new DescendantsTreeBuilder(storage, remoteFolder, localFolder, filter);
            this.eventGenerator = new CrawlEventGenerator(storage);
            this.notifier = new CrawlEventNotifier(queue);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Producer.Crawler.DescendantsCrawler"/> class based on its internal classes.
        /// This is mostly usefull for Unit Testing
        /// </summary>
        /// <param name='queue'>
        /// The event queue.
        /// </param>
        /// <param name='builder'>
        /// The DescendantsTreeBuilder.
        /// </param>
        /// <param name='generator'>
        /// The CrawlEventGenerator.
        /// </param>
        /// <param name="notifier">
        /// Event Notifier.
        /// </param>
        /// <param name='activityListener'>
        /// Activity listener.
        /// </param>
        /// <exception cref='ArgumentNullException'>
        /// <attribution license="cc4" from="Microsoft" modified="false" /><para>The exception that is thrown when a
        /// null reference (Nothing in Visual Basic) is passed to a method that does not accept it as a valid argument. </para>
        /// </exception>
        public DescendantsCrawler(
            ISyncEventQueue queue,
            IDescendantsTreeBuilder builder,
            CrawlEventGenerator generator,
            CrawlEventNotifier notifier,
            IActivityListener activityListener)
            : base(queue)
        {
            if (activityListener == null) {
                throw new ArgumentNullException("Given activityListener is null");
            }

            this.activityListener = activityListener;
            this.treebuilder = builder;
            this.eventGenerator = generator;
            this.notifier = notifier;
        }

        /// <summary>
        /// Handles StartNextSync events.
        /// </summary>
        /// <param name="e">The event to handle.</param>
        /// <returns>true if handled</returns>
        public override bool Handle(ISyncEvent e)
        {
            if (e is StartNextSyncEvent) {
                Logger.Debug("Starting DecendantsCrawlSync upon " + e);
                using (var activity = new ActivityListenerResource(this.activityListener)) {
                    this.CrawlDescendants();
                }

                this.Queue.AddEvent(new FullSyncCompletedEvent(e as StartNextSyncEvent));
                return true;
            }

            return false;
        }

        private void CrawlDescendants()
        {
            DescendantsTreeCollection trees = this.treebuilder.BuildTrees();

            CrawlEventCollection events = this.eventGenerator.GenerateEvents(trees);

            this.notifier.MergeEventsAndAddToQueue(events);
        }
    }
}
