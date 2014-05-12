//-----------------------------------------------------------------------
// <copyright file="ISyncEventManager.cs" company="GRAU DATA AG">
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
    /// <summary>
    /// Sync event manager which has a list of all Handlers and forwards events to them.
    /// </summary>
    public interface ISyncEventManager
    {
        /// <summary>
        /// Adds the event handler to the manager.
        /// </summary>
        /// <param name='handler'>
        /// Handler to add.
        /// </param>
        void AddEventHandler(SyncEventHandler handler);
  
        /// <summary>
        /// Handle the specified event.
        /// </summary>
        /// <param name='e'>
        /// Event to handle.
        /// </param>
        void Handle(ISyncEvent e);
                    
        /// <summary>
        /// Removes the event handler.
        /// </summary>
        /// <param name='handler'>
        /// Handler to remove.
        /// </param>
        void RemoveEventHandler(SyncEventHandler handler);
    }
}