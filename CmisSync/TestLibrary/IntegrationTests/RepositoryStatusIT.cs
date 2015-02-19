//-----------------------------------------------------------------------
// <copyright file="RepositoryStatusIT.cs" company="GRAU DATA AG">
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

namespace TestLibrary.IntegrationTests {
    using System;

    using CmisSync.Lib;
    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Queueing;

    using DotCMIS.Client.Impl;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class RepositoryStatusIT : IsTestWithConfiguredLog4Net {
        [Test, Category("Fast"), Ignore("TODO")]
        public void RepositoryDetectsDisconnectionAndReconnects() {
            var repoInfo = new RepoInfo();
            var listener = new ActivityListenerAggregator(Mock.Of<IActivityListener>(), new ActiveActivitiesManager());
            var underTest = new InMemoryRepo(repoInfo, listener);
        }

        private class InMemoryConnectionScheduler : CmisSync.Lib.Queueing.ConnectionScheduler {
            public InMemoryConnectionScheduler(ConnectionScheduler original, SessionFactory sessionFactory) : base(original) {
                this.SessionFactory = sessionFactory;
            }
        }

        private class InMemoryRepo : CmisSync.Lib.Cmis.Repository {
            public InMemoryRepo(RepoInfo repoInfo, ActivityListenerAggregator listener) : base(repoInfo, listener, true, CmisSync.Lib.Cmis.Repository.CreateQueue()) {

            }
        }
    }
}