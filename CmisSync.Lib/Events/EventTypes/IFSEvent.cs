//-----------------------------------------------------------------------
// <copyright file="IFSEvent.cs" company="GRAU DATA AG">
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
    using System.IO;
    
    /// <summary>
    /// Interface for FileSystem Events.
    /// </summary>
    public interface IFSEvent : ISyncEvent
    {
        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        WatcherChangeTypes Type { get; }
  
        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        string Path { get; }
  
        /// <summary>
        /// Determines whether this instance is directory.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance is directory; otherwise, <c>false</c>.
        /// </returns>
        bool IsDirectory();
    }
}