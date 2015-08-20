//-----------------------------------------------------------------------
// <copyright file="ConnectionScheduler.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Queueing {
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Events;

    using DotCMIS;
    using DotCMIS.Binding;
    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    using log4net;

    /// <summary>
    /// Connection scheduler.
    /// </summary>
    public class ConnectionScheduler : SyncEventHandler, IConnectionScheduler {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ConnectionScheduler));
        private Task task;
        private CancellationTokenSource cancelTaskSource;
        private CancellationToken cancelToken;
        private object connectionLock = new object();
        private object repoInfoLock = new object();
        private DateTime isForbiddenUntil = DateTime.MinValue;
        private DateTime? lastSuccessfulLogin = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Queueing.ConnectionScheduler"/> class.
        /// </summary>
        /// <param name="repoInfo">Repo info.</param>
        /// <param name="queue">Event queue.</param>
        /// <param name="sessionFactory">Session factory.</param>
        /// <param name="authProvider">Auth provider.</param>
        /// <param name="interval">Retry interval in msec.</param>
        public ConnectionScheduler(
            RepoInfo repoInfo,
            ISyncEventQueue queue,
            ISessionFactory sessionFactory,
            IAuthenticationProvider authProvider,
            int interval = 5000)
        {
            if (interval <= 0) {
                throw new ArgumentException(string.Format("Given Interval \"{0}\" is smaller or equal to null", interval));
            }

            if (repoInfo == null) {
                throw new ArgumentNullException("repoInfo");
            }

            if (queue == null) {
                throw new ArgumentNullException("queue");
            }

            if (sessionFactory == null) {
                throw new ArgumentNullException("sessionFactory");
            }

            if (authProvider == null) {
                throw new ArgumentNullException("authProvider");
            }

            this.Queue = queue;
            this.SessionFactory = sessionFactory;
            this.RepoInfo = repoInfo;
            this.AuthProvider = authProvider;
            this.Interval = interval;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Queueing.ConnectionScheduler"/> class by copy all members.
        /// </summary>
        /// <param name="original">Original Instance.</param>
        protected ConnectionScheduler(ConnectionScheduler original) : this(original.RepoInfo, original.Queue, original.SessionFactory, original.AuthProvider, original.Interval) {
        }

        /// <summary>
        /// Gets the interval.
        /// </summary>
        /// <value>The interval.</value>
        public int Interval { get; private set; }

        protected ISyncEventQueue Queue { get; set; }

        protected RepoInfo RepoInfo { get; set; }

        protected IAuthenticationProvider AuthProvider { get; set; }

        protected ISessionFactory SessionFactory { get; set; }

        /// <summary>
        /// Releases all resource used by the <see cref="CmisSync.Lib.Queueing.SyncScheduler"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="CmisSync.Lib.Queueing.SyncScheduler"/>.
        /// The <see cref="Dispose"/> method leaves the <see cref="CmisSync.Lib.Queueing.SyncScheduler"/> in an unusable
        /// state. After calling <see cref="Dispose"/>, you must release all references to the
        /// <see cref="CmisSync.Lib.Queueing.SyncScheduler"/> so the garbage collector can reclaim the memory that the
        /// <see cref="CmisSync.Lib.Queueing.SyncScheduler"/> was occupying.</remarks>
        public void Dispose() {
            if (this.task != null) {
                try {
                    this.cancelTaskSource.Cancel();
                    this.task.Wait(this.Interval);
                    this.task.Dispose();
                } catch (InvalidOperationException) {
                    // Disposing the login task before it is finished is not a problem
                } catch (TaskCanceledException) {
                    // It is fine if the task is canceled
                } catch (AggregateException) {
                    // It is also fine if the task is canceled
                } finally {
                    this.cancelTaskSource.Dispose();
                }
            }
        }

        /// <summary>
        /// Start this instance.
        /// </summary>
        public virtual void Start() {
            lock(this.connectionLock) {
                if (this.task == null) {
                    this.cancelTaskSource = new CancellationTokenSource();
                    this.cancelToken = this.cancelTaskSource.Token;
                    this.task = Task.Factory.StartNew(
                        () => {
                        this.cancelToken.ThrowIfCancellationRequested();
                        while (!this.cancelToken.IsCancellationRequested && !this.Connect()) {
                            this.cancelToken.WaitHandle.WaitOne(this.Interval);
                        }
                    }, this.cancelTaskSource.Token);
                }
            }
        }

        /// <summary>
        /// Handles repository configuration change events by extracting new login informations and returns false
        /// </summary>
        /// <param name="e">The event to handle.</param>
        /// <returns><c>false</c></returns>
        public override bool Handle(ISyncEvent e) {
            if (e is RepoConfigChangedEvent) {
                var changedConfig = (e as RepoConfigChangedEvent).RepoInfo;
                if (changedConfig != null) {
                    lock(this.repoInfoLock) {
                        this.RepoInfo = changedConfig;
                        // Reconnect
                        this.Reconnect();
                    }
                }
            } else if (e is CmisConnectionExceptionEvent) {
                var connectionEvent = e as CmisConnectionExceptionEvent;
                if (this.lastSuccessfulLogin != null && connectionEvent.OccuredAt > (DateTime)this.lastSuccessfulLogin) {
                    // Reconnect
                    this.Reconnect();
                }

                return true;
            }

            return false;
        }

        private void Reconnect() {
            this.isForbiddenUntil = DateTime.MinValue;
            lock(this.connectionLock) {
                if (this.task != null) {
                    try {
                        this.cancelTaskSource.Cancel();
                        this.task.Wait(this.Interval);
                        this.task.Dispose();
                    } catch (InvalidOperationException) {
                        // Disposing the login task before it is finished is not a problem.
                    } catch (TaskCanceledException) {
                        // It is also fine if the task is canceled
                    } catch (AggregateException) {
                        // It is also fine if the task is canceled
                    } finally {
                        this.cancelTaskSource.Dispose();
                        this.task = null;
                    }
                }

                this.Start();
            }
        }

        /// <summary>
        /// Connect this instance.
        /// </summary>
        /// <returns><c>true</c>, if connection was successful, otherwise <c>false</c></returns>
        protected bool Connect() {
            lock(this.repoInfoLock) {
                try {
                    if (this.isForbiddenUntil > DateTime.UtcNow) {
                        return false;
                    }

                    // Create session.
                    var session = this.SessionFactory.CreateSession(this.RepoInfo, authenticationProvider: this.AuthProvider);
                    Logger.Debug(session.RepositoryInfo.ToLogString());
                    this.cancelToken.ThrowIfCancellationRequested();
                    session.DefaultContext = OperationContextFactory.CreateDefaultContext(session);
                    this.cancelToken.ThrowIfCancellationRequested();
                    var rootFolder = session.GetObjectByPath(this.RepoInfo.RemotePath) as IFolder;
                    bool pwcSupport = session.IsPrivateWorkingCopySupported();
                    this.Queue.AddEvent(new SuccessfulLoginEvent(this.RepoInfo.Address, session, rootFolder, pwcSupport));
                    this.lastSuccessfulLogin = DateTime.Now;
                    return true;
                } catch (DotCMIS.Exceptions.CmisPermissionDeniedException e) {
                    Logger.Info(string.Format("Failed to connect to server {0}", this.RepoInfo.Address.ToString()), e);
                    var permissionDeniedEvent = new PermissionDeniedEvent(e);
                    this.Queue.AddEvent(permissionDeniedEvent);
                    this.isForbiddenUntil = permissionDeniedEvent.IsBlockedUntil ?? DateTime.MaxValue;
                } catch (CmisRuntimeException e) {
                    if (e.Message == "Proxy Authentication Required") {
                        this.Queue.AddEvent(new ProxyAuthRequiredEvent(e));
                        Logger.Warn("Proxy Settings Problem", e);
                        this.isForbiddenUntil = DateTime.MaxValue;
                    } else {
                        Logger.Error("Connection to repository failed: ", e);
                        this.Queue.AddEvent(new ExceptionEvent(e));
                    }
                } catch (DotCMIS.Exceptions.CmisInvalidArgumentException e) {
                    Logger.Warn(string.Format("Failed to connect to server {0}", this.RepoInfo.Address.ToString()), e);
                    this.Queue.AddEvent(new ConfigurationNeededEvent(e));
                    this.isForbiddenUntil = DateTime.MaxValue;
                } catch (CmisObjectNotFoundException e) {
                    Logger.Error("Failed to find cmis object: ", e);
                } catch (CmisConnectionException e) {
                    Logger.Info(string.Format("Failed to create connection to \"{0}\". Will try again in {1} ms", this.RepoInfo.Address.ToString(), this.Interval));
                    Logger.Debug(string.Empty, e);
                } catch (CmisBaseException e) {
                    Logger.Error("Failed to create session to remote " + this.RepoInfo.Address.ToString() + ": ", e);
                } catch (OperationCanceledException e) {
                    Logger.Debug("Connect to server canceled");
                }

                return false;
            }
        }
    }
}