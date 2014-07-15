//-----------------------------------------------------------------------
// <copyright file="EventHandlerPriorities.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Events
{
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Accumulator;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Events.Filter;
    using CmisSync.Lib.Sync.Strategy;

    /// <summary>
    /// Default event handler priorities.
    /// </summary>
    public static class EventHandlerPriorities
    {
        /// <summary>
        /// The DEBUG handler priority.
        /// </summary>
        public static readonly int DEBUG = 100000;

        /// <summary>
        /// The FILTER handler priority.
        /// </summary>
        public static readonly int FILTER = 10000;

        /// <summary>
        /// The HIGHER handler priority.
        /// </summary>
        public static readonly int HIGHER = 3000;

        /// <summary>
        /// The HIGH handler priority.
        /// </summary>
        public static readonly int HIGH = 2000;

        /// <summary>
        /// The NORMAL handler priority.
        /// </summary>
        public static readonly int NORMAL = 1000;

        /// <summary>
        /// The map of all known event filter types and their default priority.
        /// </summary>
        private static IDictionary<Type, int> map = new Dictionary<Type, int>();

        /// <summary>
        /// Initializes static members of the <see cref="CmisSync.Lib.Events.EventHandlerPriorities"/> class.
        /// </summary>
        static EventHandlerPriorities()
        {
            map[typeof(DebugLoggingHandler)] = DEBUG;

            map[typeof(ReportingFilter)] = FILTER;
            map[typeof(InvalidFolderNameFilter)] = FILTER;
            map[typeof(GenericHandleDublicatedEventsFilter<,>)] = FILTER;
            map[typeof(IgnoreAlreadyHandledFsEventsFilter)] = FILTER;
            map[typeof(IgnoreAlreadyHandledContentChangeEventsFilter)] = FILTER;

            // Below filter but higher than remote/local accumulators
            map[typeof(RemoteObjectMovedOrRenamedAccumulator)] = HIGHER;

            // Higher than fallback Crawler
            map[typeof(ContentChanges)] = HIGH;

            // Accumulates events needed for Transformer
            map[typeof(ContentChangeEventAccumulator)] = HIGH;

            // Accumulates events needed for SyncStrategy
            map[typeof(RemoteObjectFetcher)] = HIGH;
            map[typeof(LocalObjectFetcher)] = HIGH;

            map[typeof(ContentChangeEventTransformer)] = NORMAL;
            map[typeof(SyncScheduler)] = NORMAL;
            map[typeof(WatcherConsumer)] = NORMAL;
            map[typeof(DescendantsCrawler)] = NORMAL;
            map[typeof(SyncMechanism)] = NORMAL;
            map[typeof(GenericSyncEventHandler<>)] = NORMAL;
            map[typeof(SyncStrategyInitializer)] = NORMAL;
        }

        /// <summary>
        /// Gets the default priority of the given type
        /// </summary>
        /// <returns>
        /// The default priority.
        /// </returns>
        /// <param name='type'>
        /// Type of the event handler.
        /// </param>
        public static int GetPriority(Type type)
        {
            return map[type];
        }
    }
}
