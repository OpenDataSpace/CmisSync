//-----------------------------------------------------------------------
// <copyright file="IActivityListener.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Listen to activity/inactivity.
    /// Typically used by a status spinner:
    /// - Start spinning when activity starts
    /// - Stop spinning when activity stops
    /// </summary>
    public interface IActivityListener
    {
        /// <summary>
        /// Call this method to indicate that activity has started.
        /// </summary>
        void ActivityStarted();

        /// <summary>
        /// Call this method to indicate that activity has stopped.
        /// </summary>
        void ActivityStopped();
    }

    /// <summary>
    /// RAII class for IActivityListener
    /// </summary>
    public class ActivityListenerResource : IDisposable
    {
        private IActivityListener activityListener;

        private bool disposed = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ActivityListenerResource(IActivityListener listener)
        {
            this.activityListener = listener;
            this.activityListener.ActivityStarted();
        }

        /// <summary>
        /// Implement <code>IDisposable.Dispose</code>
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose pattern
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if(!this.disposed)
            {
                this.activityListener.ActivityStopped();
                this.disposed = true;
            }
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~ActivityListenerResource()
        {
            this.Dispose(false);
        }
    }
}
