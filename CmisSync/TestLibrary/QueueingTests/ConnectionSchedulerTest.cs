//-----------------------------------------------------------------------
// <copyright file="ConnectionSchedulerTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.QueueingTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using CmisSync.Lib.Config;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;

    using DotCMIS;
    using DotCMIS.Binding;
    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, Timeout(10000)]
    public class ConnectionSchedulerTest : IsTestWithConfiguredLog4Net
    {
        private readonly int interval = 100;
        private readonly string username = "user";
        private readonly string password = "password";
        private readonly int connectionTimeout = 10000;
        private readonly int readTimeout = 42;
        private readonly string url = "https://demo.deutsche-wolke.de/";
        private readonly AuthenticationType authType = AuthenticationType.BASIC;
        private readonly string repoId = "repoId";

        private Mock<ISyncEventQueue> queue;
        private Mock<IAuthenticationProvider> authProvider;
        private RepoInfo repoInfo;
        private Mock<ISessionFactory> sessionFactory;
        private Mock<ISession> session;

        [SetUp]
        public void SetUp() {
            this.queue = new Mock<ISyncEventQueue>();
            this.authProvider = new Mock<IAuthenticationProvider>();
            this.repoInfo = new RepoInfo {
                User = this.username,
                ConnectionTimeout = this.connectionTimeout,
                PollInterval = this.interval,
                AuthenticationType = this.authType,
                ReadTimeout = this.readTimeout,
                ObfuscatedPassword = new Password(this.password).ObfuscatedPassword,
                Address = new Uri(this.url),
                RepositoryId = this.repoId
            };

            this.session = new Mock<ISession>();
            this.session.SetupCreateOperationContext();
            this.sessionFactory = new Mock<ISessionFactory>();
        }

        [Test, Category("Fast")]
        public void ConstructorFailsIfRepoInfoIsNull() {
            Assert.Throws<ArgumentNullException>(
                () =>
                { using (new ConnectionScheduler(null, this.queue.Object, this.sessionFactory.Object, this.authProvider.Object)) {
                }
            });
        }

        [Test, Category("Fast")]
        public void ConstructorFailsIfQueueIsNull() {
            Assert.Throws<ArgumentNullException>(
                () =>
                { using (new ConnectionScheduler(this.repoInfo, null, this.sessionFactory.Object, this.authProvider.Object)) {
                }
            });
        }

        [Test, Category("Fast")]
        public void ConstructorFailsIfSessionFactoryIsNull() {
            Assert.Throws<ArgumentNullException>(
                () =>
                { using (new ConnectionScheduler(this.repoInfo, this.queue.Object, null, this.authProvider.Object)) {
                }
            });
        }

        [Test, Category("Fast")]
        public void ConstructorFailsIfAuthProviderIsNull() {
            Assert.Throws<ArgumentNullException>(
                () =>
                { using (new ConnectionScheduler(this.repoInfo, this.queue.Object, this.sessionFactory.Object, null)) {
                }
            });
        }

        [Test, Category("Fast")]
        public void ConstructorSetsDefaultInterval() {
            using (var scheduler = new ConnectionScheduler(this.repoInfo, this.queue.Object, this.sessionFactory.Object, this.authProvider.Object)) {
                Assert.That(scheduler.Interval, Is.GreaterThan(0));
            }
        }

        [Test, Category("Fast")]
        public void ConstructorTakesCustomInterval() {
            using (var scheduler = new ConnectionScheduler(this.repoInfo, this.queue.Object, this.sessionFactory.Object, this.authProvider.Object, this.interval)) {
                Assert.That(scheduler.Interval, Is.EqualTo(this.interval));
            }
        }

        [Test, Category("Fast")]
        public void DisposingWithoutHavingStarted() {
            using (new ConnectionScheduler(this.repoInfo, this.queue.Object, this.sessionFactory.Object, this.authProvider.Object, this.interval)) {
            }
        }

        [Test, Category("Fast")]
        public void DisposingWithoutHavingFinished() {
            using (var scheduler = new ConnectionScheduler(this.repoInfo, this.queue.Object, this.sessionFactory.Object, this.authProvider.Object, this.interval)) {
                scheduler.Start();
            }
        }

        [Test, Category("Fast")]
        public void CreateConnection() {
            var waitHandle = new AutoResetEvent(false);
            this.sessionFactory.Setup(f => f.CreateSession(It.IsAny<IDictionary<string, string>>(), null, this.authProvider.Object, null))
                .Callback<IDictionary<string, string>, object, object, object>(
                    (d, x, y, z) => this.VerifyConnectionProperties(d))
                    .Returns(this.session.Object);
            this.queue.Setup(q => q.AddEvent(It.IsAny<ISyncEvent>())).Callback(() => waitHandle.Set());
            using (var scheduler = new ConnectionScheduler(this.repoInfo, this.queue.Object, this.sessionFactory.Object, this.authProvider.Object, this.interval)) {
                scheduler.Start();
                waitHandle.WaitOne();
                this.queue.VerifyThatNoOtherEventIsAddedThan<SuccessfulLoginEvent>();
                this.queue.Verify(q => q.AddEvent(It.Is<SuccessfulLoginEvent>(l => l.Session == this.session.Object)));
            }

            this.session.VerifySet(s => s.DefaultContext = It.IsNotNull<IOperationContext>(), Times.Once());
        }

        [Test, Category("Fast")]
        public void CreateConnectionAndNoRetryIsExecuted() {
            var waitHandle = new AutoResetEvent(false);
            this.sessionFactory.Setup(f => f.CreateSession(It.IsAny<IDictionary<string, string>>(), null, this.authProvider.Object, null))
                .Callback<IDictionary<string, string>, object, object, object>(
                    (d, x, y, z) => this.VerifyConnectionProperties(d))
                    .Returns(this.session.Object);
            this.queue.Setup(q => q.AddEvent(It.IsAny<ISyncEvent>())).Callback(() => waitHandle.Set());
            using (var scheduler = new ConnectionScheduler(this.repoInfo, this.queue.Object, this.sessionFactory.Object, this.authProvider.Object, this.interval)) {
                scheduler.Start();
                waitHandle.WaitOne();
                this.queue.VerifyThatNoOtherEventIsAddedThan<SuccessfulLoginEvent>();
                this.queue.Verify(q => q.AddEvent(It.Is<SuccessfulLoginEvent>(l => l.Session == this.session.Object)), Times.Once());
                Assert.That(waitHandle.WaitOne(3 * this.interval), Is.False);
                this.queue.VerifyThatNoOtherEventIsAddedThan<SuccessfulLoginEvent>();
                this.queue.Verify(q => q.AddEvent(It.Is<SuccessfulLoginEvent>(l => l.Session == this.session.Object)), Times.Once());
            }

            this.session.VerifySet(s => s.DefaultContext = It.IsNotNull<IOperationContext>(), Times.Once());
        }

        [Test, Category("Fast")]
        public void LoginFailsWithPermissionDeniedException() {
            var waitHandle = new AutoResetEvent(false);
            this.sessionFactory.Setup(f => f.CreateSession(It.IsAny<IDictionary<string, string>>(), null, this.authProvider.Object, null))
                .Callback<IDictionary<string, string>, object, object, object>(
                    (d, x, y, z) => this.VerifyConnectionProperties(d)).Throws<CmisPermissionDeniedException>();
            this.queue.Setup(q => q.AddEvent(It.IsAny<ISyncEvent>())).Callback(() => waitHandle.Set());
            using (var scheduler = new ConnectionScheduler(this.repoInfo, this.queue.Object, this.sessionFactory.Object, this.authProvider.Object, this.interval)) {
                scheduler.Start();
                waitHandle.WaitOne();
                this.queue.VerifyThatNoOtherEventIsAddedThan<PermissionDeniedEvent>();
                this.queue.Verify(q => q.AddEvent(It.IsAny<PermissionDeniedEvent>()));
            }

            this.session.VerifySet(s => s.DefaultContext = It.IsAny<IOperationContext>(), Times.Never());
        }

        [Test, Category("Fast")]
        public void LoginFailsWithProxyAuthRequiredException() {
            var waitHandle = new AutoResetEvent(false);
            this.sessionFactory.Setup(f => f.CreateSession(It.IsAny<IDictionary<string, string>>(), null, this.authProvider.Object, null))
                .Callback<IDictionary<string, string>, object, object, object>(
                    (d, x, y, z) => this.VerifyConnectionProperties(d)).Throws(new CmisRuntimeException("Proxy Authentication Required"));
            this.queue.Setup(q => q.AddEvent(It.IsAny<ISyncEvent>())).Callback(() => waitHandle.Set());
            using (var scheduler = new ConnectionScheduler(this.repoInfo, this.queue.Object, this.sessionFactory.Object, this.authProvider.Object, this.interval)) {
                scheduler.Start();
                waitHandle.WaitOne();
                this.queue.VerifyThatNoOtherEventIsAddedThan<ProxyAuthRequiredEvent>();
                this.queue.Verify(q => q.AddEvent(It.IsAny<ProxyAuthRequiredEvent>()));
            }

            this.session.VerifySet(s => s.DefaultContext = It.IsAny<IOperationContext>(), Times.Never());
        }

        [Test, Category("Fast")]
        public void LoginRetryOccursIfFirstLoginFailed() {
            var waitHandle = new AutoResetEvent(false);
            this.sessionFactory.Setup(f => f.CreateSession(It.IsAny<IDictionary<string, string>>(), null, this.authProvider.Object, null))
                .Callback<IDictionary<string, string>, object, object, object>(
                    (d, x, y, z) => this.VerifyConnectionProperties(d)).Throws(new CmisRuntimeException("Proxy Authentication Required"));
            this.queue.Setup(q => q.AddEvent(It.IsAny<ISyncEvent>())).Callback(() => waitHandle.Set());
            using (var scheduler = new ConnectionScheduler(this.repoInfo, this.queue.Object, this.sessionFactory.Object, this.authProvider.Object, this.interval)) {
                scheduler.Start();
                waitHandle.WaitOne();
                this.queue.Verify(q => q.AddEvent(It.IsAny<ProxyAuthRequiredEvent>()), Times.Once());
                this.sessionFactory.Setup(f => f.CreateSession(It.IsAny<IDictionary<string, string>>(), null, this.authProvider.Object, null))
                    .Callback<IDictionary<string, string>, object, object, object>(
                        (d, x, y, z) => this.VerifyConnectionProperties(d)).Returns(this.session.Object);
                waitHandle.WaitOne();
                this.queue.Verify(q => q.AddEvent(It.Is<SuccessfulLoginEvent>(l => l.Session == this.session.Object)), Times.Once());
                Assert.That(waitHandle.WaitOne(3 * this.interval), Is.False);
                this.queue.Verify(q => q.AddEvent(It.Is<SuccessfulLoginEvent>(l => l.Session == this.session.Object)), Times.Once());
                this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Exactly(2));
            }

            this.session.VerifySet(s => s.DefaultContext = It.IsNotNull<IOperationContext>(), Times.Once());
        }

        private bool VerifyConnectionProperties(IDictionary<string, string> d)
        {
            Assert.That(d.ContainsKey(SessionParameter.User));
            Assert.That(d[SessionParameter.User], Is.EqualTo(this.username));
            Assert.That(d.ContainsKey(SessionParameter.Password));
            Assert.That(d[SessionParameter.Password], Is.EqualTo(this.password));
            Assert.That(d.ContainsKey(SessionParameter.UserAgent));
            Assert.That(d.ContainsKey(SessionParameter.ConnectTimeout));
            Assert.That(d[SessionParameter.ConnectTimeout], Is.EqualTo(this.connectionTimeout.ToString()));
            Assert.That(d.ContainsKey(SessionParameter.ReadTimeout));
            Assert.That(d[SessionParameter.ReadTimeout], Is.EqualTo(this.readTimeout.ToString()));
            Assert.That(d.ContainsKey(SessionParameter.RepositoryId));
            Assert.That(d[SessionParameter.RepositoryId], Is.EqualTo(this.repoId));
            Assert.That(d.ContainsKey(SessionParameter.BindingType));
            if (d[SessionParameter.BindingType] == BindingType.AtomPub) {
                Assert.That(d[SessionParameter.AtomPubUrl], Is.EqualTo(this.url));
            }

            return true;
        }
    }
}