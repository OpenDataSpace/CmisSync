//-----------------------------------------------------------------------
// <copyright file="SessionFactoryExtenders.cs" company="GRAU DATA AG">
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
﻿
namespace CmisSync.Lib.Cmis.ConvenienceExtenders {
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Config;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Binding;
    using DotCMIS.Client.Impl.Cache;

    /// <summary>
    /// DotCMIS Session factory extenders.
    /// </summary>
    public static class SessionFactoryExtenders {
        /// <summary>
        /// Creates a session.
        /// </summary>
        /// <returns>The session.</returns>
        /// <param name="factory">Session factory.</param>
        /// <param name="repoInfo">Repo informations.</param>
        /// <param name="objectFactory">Object factory.</param>
        /// <param name="authenticationProvider">Authentication provider.</param>
        /// <param name="cache">Object cache.</param>
        /// <param name="appName">App name.</param>
        public static ISession CreateSession(
            this ISessionFactory factory,
            RepoInfo repoInfo,
            IObjectFactory objectFactory = null,
            IAuthenticationProvider authenticationProvider = null,
            ICache cache = null,
            string appName = null)
        {
            if (factory == null) {
                throw new ArgumentNullException("factory");
            }

            return factory.CreateSession(repoInfo.AsSessionParameter(appName), objectFactory, authenticationProvider, cache);
        }

        /// <summary>
        /// Creates a session.
        /// </summary>
        /// <returns>The session.</returns>
        /// <param name="factory">Session factory.</param>
        /// <param name="repoInfo">Repo informations.</param>
        /// <param name="appName">App name.</param>
        public static ISession CreateSession(
            this ISessionFactory factory,
            RepoInfo repoInfo,
            string appName = null)
        {
            if (factory == null) {
                throw new ArgumentNullException("factory");
            }

            return factory.CreateSession(repoInfo.AsSessionParameter(appName));
        }

        /// <summary>
        /// Extracts all CMIS session parameter from repository informations.
        /// </summary>
        /// <returns>CMIS session parameter based on given repo infos.</returns>
        /// <param name="repoInfo">Repository informations.</param>
        /// <param name="appName">App name.</param>
        public static Dictionary<string, string> AsSessionParameter(this RepoInfo repoInfo, string appName = null) {
            if (repoInfo == null) {
                throw new ArgumentNullException("repoInfo");
            }

            var parameters = new Dictionary<string, string>();
            if (repoInfo.Binding == DotCMIS.BindingType.AtomPub) {
                parameters[SessionParameter.BindingType] = BindingType.AtomPub;
                parameters[SessionParameter.AtomPubUrl] = repoInfo.Address.ToString();
            } else if (repoInfo.Binding == DotCMIS.BindingType.Browser) {
                parameters[SessionParameter.BindingType] = BindingType.Browser;
                parameters[SessionParameter.BrowserUrl] = repoInfo.Address.ToString();
            }

            parameters[SessionParameter.User] = repoInfo.User;
            parameters[SessionParameter.Password] = repoInfo.GetPassword().ToString();
            parameters[SessionParameter.RepositoryId] = repoInfo.RepositoryId;
            parameters[SessionParameter.ConnectTimeout] = repoInfo.ConnectionTimeout.ToString();
            parameters[SessionParameter.ReadTimeout] = repoInfo.ReadTimeout.ToString();
            parameters[SessionParameter.DeviceIdentifier] = ConfigManager.CurrentConfig.DeviceId.ToString();
            parameters[SessionParameter.UserAgent] = appName != null ? Utils.CreateUserAgent(appName) : Utils.CreateUserAgent();
            parameters[SessionParameter.Compression] = bool.TrueString;
            parameters[SessionParameter.MaximumRequestRetries] = repoInfo.HttpMaximumRetries.ToString();
            return parameters;
        }
    }
}