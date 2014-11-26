//-----------------------------------------------------------------------
// <copyright file="IgnoredEntitiesCollection.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.SelectiveIgnore
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;

    public class IgnoredEntitiesCollection : IIgnoredEntitiesStorage
    {
        public void Add(IIgnoredEntity ignore) {
            throw new NotImplementedException();
        }

        public void Remove(IIgnoredEntity ignore) {
            throw new NotImplementedException();
        }

        public IgnoredState IsIgnoredId(string objectId) {
            throw new NotImplementedException();
        }

        public IgnoredState IsIgnoredPath(string localPath) {
            throw new NotImplementedException();
        }
    }
}