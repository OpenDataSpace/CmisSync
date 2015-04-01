
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
        private static Dictionary<string, string> GetRepositoriesCmisSessionParameter(this ServerCredentials credentials, int timeout = 5000) {
            Dictionary<string, string> cmisParameters = new Dictionary<string, string>();
            if (credentials.Binding == DotCMIS.BindingType.AtomPub) {
                cmisParameters[SessionParameter.BindingType] = BindingType.AtomPub;
                cmisParameters[SessionParameter.AtomPubUrl] = credentials.Address.ToString();
            } else if (credentials.Binding == DotCMIS.BindingType.Browser) {
                cmisParameters[SessionParameter.BindingType] = BindingType.Browser;
                cmisParameters[SessionParameter.BrowserUrl] = credentials.Address.ToString();
            }

            cmisParameters[SessionParameter.User] = credentials.UserName;
            cmisParameters[SessionParameter.Password] = credentials.Password.ToString();
            cmisParameters[SessionParameter.DeviceIdentifier] = ConfigManager.CurrentConfig.DeviceId.ToString();
            cmisParameters[SessionParameter.UserAgent] = Utils.CreateUserAgent();
            cmisParameters[SessionParameter.Compression] = bool.TrueString;
            cmisParameters[SessionParameter.ConnectTimeout] = timeout.ToString();
            cmisParameters[SessionParameter.ReadTimeout] = timeout.ToString();
            return cmisParameters;
        }

        /// <summary>
        /// Get the list of repositories of a CMIS server
        /// Each item contains id + 
        /// </summary>
        /// <returns>The list of repositories. Each item contains the identifier and the human-readable name of the repository.</returns>
        public static IList<LogonRepositoryInfo> GetRepositories(this ServerCredentials credentials, ISessionFactory sessionFactory = null) {
            var result = new List<LogonRepositoryInfo>();
            // If no URL was provided, return empty result.
            if (credentials.Address == null) {
                return result;
            }

            // Create session factory.
            var factory = sessionFactory ?? SessionFactory.NewInstance();
            var cmisParameters = credentials.GetRepositoriesCmisSessionParameter();
            IList<IRepository> repositories = factory.GetRepositories(cmisParameters);

            // Populate the result list with identifier and name of each repository.
            foreach (var repo in repositories) {
                result.Add(new LogonRepositoryInfo(repo.Id, repo.Name));
            }

            return result;
        }

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