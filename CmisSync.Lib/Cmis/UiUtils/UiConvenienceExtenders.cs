
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