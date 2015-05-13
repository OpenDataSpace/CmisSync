//-----------------------------------------------------------------------
// <copyright file="ServerCredentials.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Config {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Cmis.UiUtils;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl;
    using DotCMIS;

    /// <summary>
    /// Server Login for a specific Uri
    /// </summary>
    [Serializable]
    public class ServerCredentials : UserCredentials {
        /// <summary>
        /// Atom pub binding string.
        /// </summary>
        public static readonly string BindingAtomPub = DotCMIS.BindingType.AtomPub;

        /// <summary>
        /// Browser binding string.
        /// </summary>
        public static readonly string BindingBrowser = DotCMIS.BindingType.Browser;

        private string binding = BindingAtomPub;

        /// <summary>
        /// Gets or sets the server Address and Path
        /// </summary>
        public Uri Address { get; set; }

        /// <summary>
        /// Gets or sets the CMIS binding
        /// </summary>
        public string Binding {
            get {
                return this.binding;
            }

            set {
                this.binding = value;
            }
        }

        public IList<CmisSync.Lib.Cmis.UiUtils.LogonRepositoryInfo> GetRepositories() {
            return this.GetRepositories(null);
        }

        /// <summary>
        /// Get the list of repositories of a CMIS server
        /// </summary>
        /// <returns>The list of repositories. Each item contains the identifier and the human-readable name of the repository.</returns>
        public IList<CmisSync.Lib.Cmis.UiUtils.LogonRepositoryInfo> GetRepositories(ISessionFactory sessionFactory) {
            var result = new List<CmisSync.Lib.Cmis.UiUtils.LogonRepositoryInfo>();
            // If no URL was provided, return empty result.
            if (this.Address == null) {
                return result;
            }

            // Create session factory.
            var factory = sessionFactory ?? SessionFactory.NewInstance();
            var cmisParameters = this.GetRepositoriesCmisSessionParameter();
            IList<IRepository> repositories = factory.GetRepositories(cmisParameters);

            // Populate the result list with identifier and name of each repository.
            foreach (var repo in repositories) {
                result.Add(new CmisSync.Lib.Cmis.UiUtils.LogonRepositoryInfo(repo.Id, repo.Name));
            }

            return result;
        }

        public override string ToString() {
            return string.Format("[ServerCredentials: Address={0}, Binding={1}, UserName={2}]", Address, Binding, UserName);
        }

        private Dictionary<string, string> GetRepositoriesCmisSessionParameter(int timeout = 5000) {
            Dictionary<string, string> cmisParameters = new Dictionary<string, string>();
            if (this.Binding == DotCMIS.BindingType.AtomPub) {
                cmisParameters[SessionParameter.BindingType] = BindingType.AtomPub;
                cmisParameters[SessionParameter.AtomPubUrl] = this.Address.ToString();
            } else if (this.Binding == DotCMIS.BindingType.Browser) {
                cmisParameters[SessionParameter.BindingType] = BindingType.Browser;
                cmisParameters[SessionParameter.BrowserUrl] = this.Address.ToString();
            }

            cmisParameters[SessionParameter.User] = this.UserName;
            cmisParameters[SessionParameter.Password] = this.Password.ToString();
            cmisParameters[SessionParameter.DeviceIdentifier] = ConfigManager.CurrentConfig.DeviceId.ToString();
            cmisParameters[SessionParameter.UserAgent] = Utils.CreateUserAgent();
            cmisParameters[SessionParameter.Compression] = bool.TrueString;
            cmisParameters[SessionParameter.ConnectTimeout] = timeout.ToString();
            cmisParameters[SessionParameter.ReadTimeout] = timeout.ToString();
            return cmisParameters;
        }
    }
}