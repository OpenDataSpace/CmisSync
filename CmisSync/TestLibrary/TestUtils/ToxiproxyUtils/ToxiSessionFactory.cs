//-----------------------------------------------------------------------
// <copyright file="ToxiSessionFactory.cs" company="GRAU DATA AG">
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
namespace TestLibrary.TestUtils.ToxiproxyUtils {
    using System;
    using System.Collections.Generic;

    using DotCMIS;
    using DotCMIS.Binding;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl.Cache;

    public class ToxiSessionFactory : ISessionFactory {
        private readonly ISessionFactory original;

        public string Host { get; set; }

        public int Port { get; set; }

        public ToxiSessionFactory(ISessionFactory origin) {
            if (origin == null) {
                throw new ArgumentNullException("origin");
            }

            this.original = origin;
        }

        public ISession CreateSession(IDictionary<string, string> parameters) {
            return this.original.CreateSession(this.ReplaceHostAndPort(parameters));
        }

        public ISession CreateSession(IDictionary<string, string> parameters, IObjectFactory objectFactory, IAuthenticationProvider authenticationProvider, ICache cache) {
            return this.original.CreateSession(this.ReplaceHostAndPort(parameters), objectFactory, authenticationProvider, cache);
        }

        public IList<IRepository> GetRepositories(IDictionary<string, string> parameters) {
            return this.original.GetRepositories(this.ReplaceHostAndPort(parameters));
        }

        private IDictionary<string, string> ReplaceHostAndPort(IDictionary<string, string> parameters) {
            var dict = new Dictionary<string, string>(parameters);
            if (dict.ContainsKey(SessionParameter.BrowserUrl)) {
                dict[SessionParameter.BrowserUrl] = this.Replace(dict[SessionParameter.BrowserUrl]);
            }

            if (dict.ContainsKey(SessionParameter.AtomPubUrl)) {
                dict[SessionParameter.AtomPubUrl] = this.Replace(dict[SessionParameter.AtomPubUrl]);
            }

            return dict;
        }

        private string Replace(string uri) {
            var builder = new UriBuilder(uri);
            builder.Host = this.Host;
            builder.Port = this.Port;
            return builder.Uri.ToString();
        }
    }
}