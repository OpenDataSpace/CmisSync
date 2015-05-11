//-----------------------------------------------------------------------
// <copyright file="MockedFolderType.cs" company="GRAU DATA AG">
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

namespace TestLibrary.MockedServer {
    using System;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Enums;

    using Moq;

    public class MockedFolderType : MockedObjectType<IFolderType> {
        public MockedFolderType(bool modificationDateUpdatable = true, MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.Id = BaseTypeId.CmisFolder.GetCmisValue();
            this.BaseType = this.Object;
            this.IsBaseType = true;
            this.BaseTypeId = BaseTypeId.CmisFolder;
            this.Description = "mocked C# folder type";
            this.DisplayName = "mocked folder type";
            var modificationDateDefinition = new MockedPropertyDateTimeDefinition() {
                Id = PropertyIds.LastModificationDate,
                Updatability = modificationDateUpdatable ? Updatability.ReadWrite : Updatability.ReadOnly
            }.Object;
            this.PropertyDefinitions.Add(modificationDateDefinition);
        }
    }
}