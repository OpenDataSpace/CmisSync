//-----------------------------------------------------------------------
// <copyright file="MockOfICmisObject.cs" company="GRAU DATA AG">
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

namespace TestLibrary.TestUtils {
    using System;
    using System.Collections.Generic;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Data;

    using Moq;

    public static class MockOfICmisObject {
        public static void SetupReadOnly(this Mock<ICmisObject> mock, bool readOnly = true) {
            var actions = new List<string>();
            actions.Add(Actions.CanGetAcl);
            actions.Add(Actions.CanGetAppliedPolicies);
            if (mock.Object is IDocument) {
                actions.Add(Actions.CanGetAllVersions);
                actions.Add(Actions.CanGetContentStream);
            }

            if (mock.Object is IFolder) {
                actions.Add(Actions.CanGetChildren);
            }

            if (!readOnly) {
                actions.Add(Actions.CanUpdateProperties);
                actions.Add(Actions.CanMoveObject);
                actions.Add(Actions.CanDeleteObject);
                actions.Add(Actions.CanApplyAcl);
                if (mock.Object is IDocument) {
                    actions.Add(Actions.CanSetContentStream);
                    actions.Add(Actions.CanDeleteContentStream);
                }

                if (mock.Object is IFolder) {
                    actions.Add(Actions.CanCreateDocument);
                    actions.Add(Actions.CanCreateFolder);
                    actions.Add(Actions.CanDeleteTree);
                }
            }

            mock.SetupAllowableActions(actions.ToArray());
        }

        public static void SetupReadOnly(this Mock<IDocument> doc, bool readOnly = true) {
            var actions = new List<string>();
            actions.Add(Actions.CanGetAcl);
            actions.Add(Actions.CanGetAppliedPolicies);
            actions.Add(Actions.CanGetAllVersions);
            actions.Add(Actions.CanGetContentStream);

            if (!readOnly) {
                actions.Add(Actions.CanUpdateProperties);
                actions.Add(Actions.CanMoveObject);
                actions.Add(Actions.CanDeleteObject);
                actions.Add(Actions.CanApplyAcl);
                actions.Add(Actions.CanSetContentStream);
                actions.Add(Actions.CanDeleteContentStream);
            }

            doc.SetupAllowableActions(actions.ToArray());
        }

        public static void SetupReadOnly(this Mock<IFolder> folder, bool readOnly = true) {
            var actions = new List<string>();
            actions.Add(Actions.CanGetAcl);
            actions.Add(Actions.CanGetAppliedPolicies);
            actions.Add(Actions.CanGetChildren);

            if (!readOnly) {
                actions.Add(Actions.CanUpdateProperties);
                actions.Add(Actions.CanMoveObject);
                actions.Add(Actions.CanDeleteObject);
                actions.Add(Actions.CanApplyAcl);
                actions.Add(Actions.CanCreateDocument);
                actions.Add(Actions.CanCreateFolder);
                actions.Add(Actions.CanDeleteTree);
            }

            folder.SetupAllowableActions(actions.ToArray());
        }

        public static void SetupAllowableActions(this Mock<ICmisObject> mock, params string[] actions) {
            var allowableActions = new HashSet<string>(actions);
            var allowableMock = Mock.Of<IAllowableActions>(o => o.Actions == allowableActions);
            mock.Setup(m => m.AllowableActions).Returns(allowableMock);
        }

        public static void SetupAllowableActions(this Mock<IDocument> doc, params string[] actions) {
            var allowableActions = new HashSet<string>(actions);
            var allowableMock = Mock.Of<IAllowableActions>(o => o.Actions == allowableActions);
            doc.Setup(m => m.AllowableActions).Returns(allowableMock);
        }

        public static void SetupAllowableActions(this Mock<IFolder> folder, params string[] actions) {
            var allowableActions = new HashSet<string>(actions);
            var allowableMock = Mock.Of<IAllowableActions>(o => o.Actions == allowableActions);
            folder.Setup(m => m.AllowableActions).Returns(allowableMock);
        }
    }
}