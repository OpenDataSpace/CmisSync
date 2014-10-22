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

    public class IgnoredEntitiesCollection : ObservableCollection<IIgnoredEntity>
    {
        private Dictionary<string, string> ignores = new Dictionary<string, string>();
        public IgnoredEntitiesCollection() : base() {
        }

        public IgnoredEntitiesCollection(List<IIgnoredEntity> ignores) : base(ignores) {
            foreach (var ignore in ignores) {
                this.ignores.Add(ignore.ObjectId, ignore.LocalPath);
            }
        }

        public IgnoredEntitiesCollection(IEnumerable<IIgnoredEntity> ignores) : base(ignores) {

        }

        public bool ContainsObjectId(string objectId) {
            return false;
        }
    }
}