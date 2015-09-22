//-----------------------------------------------------------------------
// <copyright file="MockedSession.cs" company="GRAU DATA AG">
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
    using System.Collections.Generic;
    using System.Linq;

    using DotCMIS;
    using DotCMIS.Binding;
    using DotCMIS.Binding.Services;
    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Data.Extensions;
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;

    using Moq;

    using TestLibrary.TestUtils;

    public class MockedSession : Mock<ISession> {
        public MockedSession(string repoId, MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.Objects = new Dictionary<string, ICmisObject>();
            this.Setup(s => s.ObjectFactory).Returns(() => this.ObjectFactory);
            this.Setup(s => s.Binding).Returns(() => this.Binding);
            this.Setup(s => s.RepositoryInfo).Returns(() => this.Binding.GetRepositoryService().GetRepositoryInfo(repoId, null));

            this.Setup(s => s.Delete(It.Is<IObjectId>(o => this.Objects.ContainsKey(o.Id)))).Callback<IObjectId>((o) => this.Delete(o.Id));
            this.Setup(s => s.Delete(It.Is<IObjectId>(o => this.Objects.ContainsKey(o.Id)), It.IsAny<bool>())).Callback<IObjectId, bool>((o, a) => this.Delete(o.Id));

            this.Setup(s => s.GetContentStream(It.Is<IObjectId>(o => (this.Objects[o.Id] as IDocument) != null))).Returns<IObjectId>((o) => (this.Objects[o.Id] as IDocument).GetContentStream());

            this.Setup(s => s.GetRootFolder()).Returns(() => this.GetObject(this.Object.RepositoryInfo.RootFolderId) as IFolder);
            this.Setup(s => s.GetRootFolder(It.IsAny<IOperationContext>())).Returns(() => this.GetObject(this.Object.RepositoryInfo.RootFolderId) as IFolder);

            this.Setup(s => s.GetObjectByPath(It.Is<string>(p => !string.IsNullOrEmpty(p)))).Returns<string>((p) => this.GetObjectByPath(p));
            this.Setup(s => s.GetObjectByPath(It.Is<string>(p => !string.IsNullOrEmpty(p)), It.IsAny<IOperationContext>())).Returns<string, IOperationContext>((p, c) => this.GetObjectByPath(p));

            this.Setup(s => s.GetObject(It.IsAny<string>())).Returns<string>((objectId) => this.GetObject(objectId));
            this.Setup(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>())).Returns<string, IOperationContext>((objectId, ctx) => this.GetObject(objectId));
            this.Setup(s => s.GetObject(It.IsAny<IObjectId>())).Returns<IObjectId>((objectId) => this.GetObject(objectId.Id));
            this.Setup(s => s.GetObject(It.IsAny<IObjectId>(), It.IsAny<IOperationContext>())).Returns<IObjectId, IOperationContext>((objectId, ctx) => this.GetObject(objectId.Id));
        }

        public Dictionary<string, ICmisObject> Objects { get; set; }

        public ICmisBinding Binding { get; set; }

        public IObjectFactory ObjectFactory { get; set; }

        public void Delete(string objectId) {
            if (!this.Objects.Remove(objectId)) {
                throw new CmisObjectNotFoundException();
            }
        }

        public void AddObjects(params ICmisObject[] objects) {
            foreach (var obj in objects) {
                this.Objects[obj.Id] = obj;
            }
        }

        private ICmisObject GetObjectByPath(string path) {
            var obj = this.Objects.First((o) => (o.Value is IFileableCmisObject && (o.Value as IFileableCmisObject).Paths.Contains(path))).Value;
            if (obj == null) {
                throw new CmisObjectNotFoundException();
            } else {
                return obj;
            }
        }

        private ICmisObject GetObject(string objectId) {
            if (!this.Objects.ContainsKey(objectId)) {
                throw new CmisObjectNotFoundException();
            }

            return this.Objects[objectId];
        }
    }
}