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

namespace CmisSync.Lib.Queueing
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;

    using CmisSync.Lib.Cmis;
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
    public class ConnectionScheduler : SyncEventHandler, IConnectionScheduler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ConnectionScheduler));
        private Task task;
        private CancellationTokenSource cancelTaskSource;
        private CancellationToken cancelToken;
        private object connectionLock = new object();
        private object repoInfoLock = new object();
        private DateTime isForbiddenUntil = DateTime.MinValue;

        protected ISyncEventQueue Queue { get; set; }

        protected RepoInfo RepoInfo { get; set; }

        protected IAuthenticationProvider AuthProvider { get; set; }

        protected ISessionFactory SessionFactory { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Queueing.ConnectionScheduler"/> class.
        /// </summary>
        /// <param name="repoInfo">Repo info.</param>
        /// <param name="queue">Queue.</param>
        /// <param name="sessionFactory">Session factory.</param>
        /// <param name="authProvider">Auth provider.</param>
        /// <param name="interval">Interval.</param>
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
                throw new ArgumentNullException("Given repo info is null");
            }

            if (queue == null) {
                throw new ArgumentNullException("Given queue is null");
            }

            if (sessionFactory == null) {
                throw new ArgumentNullException("Given session factory is null");
            }

            if (authProvider == null) {
                throw new ArgumentNullException("Given authentication provider is null");
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
        protected ConnectionScheduler(ConnectionScheduler original) : this(original.RepoInfo, original.Queue, original.SessionFactory, original.AuthProvider, original.Interval){
        }

        /// <summary>
        /// Gets the interval.
        /// </summary>
        /// <value>The interval.</value>
        public int Interval { get; private set; }

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
        /// <returns>false</returns>
        public override bool Handle(ISyncEvent e)
        {
            if (e is RepoConfigChangedEvent) {
                var changedConfig = (e as RepoConfigChangedEvent).RepoInfo;
                if (changedConfig != null) {
                    lock(this.repoInfoLock) {
                        this.RepoInfo = changedConfig;
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
                                    this.task = null;
                                }
                            }

                            this.Start();
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Connect this instance.
        /// </summary>
        protected bool Connect()
        {
            lock(this.repoInfoLock) {
                try {
                    if (this.isForbiddenUntil > DateTime.UtcNow) {
                        return false;
                    }

                    // Create session.
                    var session = this.SessionFactory.CreateSession(this.GetCmisParameter(this.RepoInfo), null, this.AuthProvider, null);
                    this.cancelToken.ThrowIfCancellationRequested();
                    session.DefaultContext = OperationContextFactory.CreateDefaultContext(session);
                    this.cancelToken.ThrowIfCancellationRequested();
                    this.Queue.AddEvent(new SuccessfulLoginEvent(this.RepoInfo.Address, session));
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
                } catch (CmisObjectNotFoundException e) {
                    Logger.Error("Failed to find cmis object: ", e);
                } catch (CmisBaseException e) {
                    Logger.Error("Failed to create session to remote " + this.RepoInfo.Address.ToString() + ": ", e);
                }

                return false;
            }
        }

        /// <summary>
        /// Parameter to use for all CMIS requests.
        /// </summary>
        /// <returns>
        /// The cmis parameter.
        /// </returns>
        /// <param name='repoInfo'>
        /// The repository infos.
        /// </param>
        private Dictionary<string, string> GetCmisParameter(RepoInfo repoInfo)
        {
            Dictionary<string, string> cmisParameters = new Dictionary<string, string>();
            cmisParameters[SessionParameter.BindingType] = BindingType.AtomPub;
            cmisParameters[SessionParameter.AtomPubUrl] = repoInfo.Address.ToString();
            cmisParameters[SessionParameter.User] = repoInfo.User;
            cmisParameters[SessionParameter.Password] = repoInfo.GetPassword().ToString();
            cmisParameters[SessionParameter.RepositoryId] = repoInfo.RepositoryId;
            cmisParameters[SessionParameter.ConnectTimeout] = repoInfo.ConnectionTimeout.ToString();
            cmisParameters[SessionParameter.ReadTimeout] = repoInfo.ReadTimeout.ToString();
            cmisParameters[SessionParameter.DeviceIdentifier] = ConfigManager.CurrentConfig.DeviceId.ToString();
            cmisParameters[SessionParameter.UserAgent] = Utils.CreateUserAgent();
            cmisParameters[SessionParameter.Compression] = bool.TrueString;
            return cmisParameters;
        }
    }
}