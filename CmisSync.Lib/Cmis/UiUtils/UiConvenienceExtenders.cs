//-----------------------------------------------------------------------
// <copyright file="UiConvenienceExtenders.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Cmis.UiUtils {
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Config;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl;
    using DotCMIS.Exceptions;

    /// <summary>
    /// User interface convenience extenders.
    /// </summary>
    public static class UiConvenienceExtenders {
        public static IList<LogonRepositoryInfo> WithoutHiddenOnce(this IList<LogonRepositoryInfo> repositories, IList<string> hiddenNames = null) {
            var result = new List<LogonRepositoryInfo>();
            hiddenNames = hiddenNames ?? ConfigManager.CurrentConfig.HiddenRepoNames;
            foreach (var repo in repositories) {
                if (!Utils.IsRepoNameHidden(repo.Name, hiddenNames)) {
                    result.Add(repo);
                }
            }

            return result;
        }

        public static List<LoginCredentials> CreateFuzzyCredentials(this ServerCredentials normalCredentials) {
            var result = new List<LoginCredentials>();
            result.Add(new LoginCredentials {
                Credentials = normalCredentials
            });
            result.Add(new LoginCredentials {
                Credentials = new ServerCredentials {
                    Address = new Uri(normalCredentials.Address.ToString()),
                    Password = new Password(normalCredentials.Password.ToString()),
                    Binding = normalCredentials.Binding == BindingType.AtomPub ? BindingType.Browser : BindingType.AtomPub,
                    UserName = normalCredentials.UserName
                }
            });

            string[] bindings = {
                BindingType.Browser,
                BindingType.AtomPub
            };
            string[] browserSuffixes = {
                "/cmis/browser"
            };
            string[] atompubSuffixes = {
                "/cmis/atom11",
                "/cmis/atom"
            };

            // Extract protocol and server name or IP address
            string prefix = normalCredentials.Address.GetLeftPart(UriPartial.Authority);

            string[][] suffixes = {
                browserSuffixes,
                atompubSuffixes
            };

            for (int i = 0; i < bindings.Length; ++i) {
                // Create all suffixes
                for (int j = 0; j < suffixes[i].Length; ++j) {
                    string fuzzyUrl = prefix + suffixes[i][j];
                    result.Add(new LoginCredentials {
                        Credentials = new ServerCredentials {
                            Address = new Uri(fuzzyUrl),
                            Binding = bindings[i],
                            UserName = normalCredentials.UserName,
                            Password = new Password(normalCredentials.Password.ToString())
                        }
                    });
                }
            }

            return result;
        }
    }
}