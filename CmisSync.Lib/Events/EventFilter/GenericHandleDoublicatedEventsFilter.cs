//-----------------------------------------------------------------------
// <copyright file="GenericHandleDoublicatedEventsFilter.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Events.Filter
{
    using System;

    using CmisSync.Lib.Events;

    /// <summary>
    /// Generic handle dublicated events filter.
    /// </summary>
    /// <typeparam name="TFilter">
    /// Events of this Type will be filtered until the second occurence
    /// </typeparam>
    /// <typeparam name="TReset">
    /// If an ISyncEvent with this type is passed this resets the filter once.
    /// </typeparam>
    public class GenericHandleDoublicatedEventsFilter<TFilter, TReset> : SyncEventHandler
        where TFilter : ISyncEvent
        where TReset : ISyncEvent
    {
        /// <summary>
        /// The first occurence of the given event class type.
        /// </summary>
        private bool firstOccurence = true;

        /// <summary>
        /// Gets the default priority of all GenericHandleDublicatedEventsFilter
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public override int Priority
        {
            get
            {
                return EventHandlerPriorities.GetPriority(typeof(GenericHandleDoublicatedEventsFilter<,>));
            }
        }

        /// <summary>
        /// Tries to handle the specified event if it has the same type like TFilter and it already occured.
        /// </summary>
        /// <param name='e'>
        /// Occured Event.
        /// </param>
        /// <returns>
        /// True, if this Event Type has already been seen since the last Reset Type or Initialization occured.
        /// </returns>
        public override bool Handle(ISyncEvent e)
        {
            if (e is TFilter)
            {
                if (this.firstOccurence)
                {
                    this.firstOccurence = false;
                    return false;
                }
                else
                {
                    return true;
                }
            }

            if (e is TReset)
            {
                this.firstOccurence = true;
            }

            return false;
        }
    }
}
