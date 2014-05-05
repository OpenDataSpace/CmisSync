//-----------------------------------------------------------------------
// <copyright file="FSEvent.cs" company="GRAU DATA AG">
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
    using System.IO;

    public class FSEvent : ISyncEvent
    {
        public WatcherChangeTypes Type { get; private set; }

        public virtual string Path { get; private set; }

        public FSEvent(WatcherChangeTypes type, string path)
        {
            if (path == null) {
                throw new ArgumentNullException("Argument null in FSEvent Constructor", "path");
            }

            this.Type = type;
            this.Path = path;
        }

        public override string ToString()
        {
            return string.Format("FSEvent with type \"{0}\" on path \"{1}\"", this.Type, this.Path);
        }

        public virtual bool IsDirectory()
        {
            // detect whether its a directory or file
            try
            {
                return (File.GetAttributes(this.Path) & FileAttributes.Directory) == FileAttributes.Directory;
            }
            catch(IOException)
            {
                return false;
            }
        }
    }
}