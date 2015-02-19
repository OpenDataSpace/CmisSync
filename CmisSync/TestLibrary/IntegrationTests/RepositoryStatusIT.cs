
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