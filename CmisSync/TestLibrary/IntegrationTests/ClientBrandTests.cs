//-----------------------------------------------------------------------
// <copyright file="ClientBrandTests.cs" company="GRAU DATA AG">
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

namespace TestLibrary.IntegrationTests {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;

    using CmisSync.Lib;
    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Cmis.UiUtils;
    using CmisSync.Lib.Config;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl;
    using DotCMIS.Data.Impl;
    using DotCMIS.Enums;

    using log4net;

    using NUnit.Framework;

    using TestUtils;

    /// <summary>
    /// Client Brand integration tests. Each method tests one specific test case. The test got to be finished after 1 min, otherwise the test will fail.
    /// </summary>
    [TestFixture, Timeout(60000), Category("Branding"), Category("Slow")]
    class ClientBrandTests : IsTestWithConfiguredLog4Net {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ClientBrandTests));

        /// <summary>
        /// Disable HTTPS Verification
        /// </summary>
        [TestFixtureSetUp]
        public void ClassInit() {
#if __MonoCS__
            Environment.SetEnvironmentVariable("MONO_XMLSERIALIZER_THS", "no");
#endif
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        /// <summary>
        /// Reanable HTTPS Verification
        /// </summary>
        [TestFixtureTearDown]
        public void FixtureTearDown() {
            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        /// <summary>
        /// Test CMIS server connection
        /// </summary>
        [Test, TestCaseSource(typeof(ITUtils), "TestServers")]
        public void TestServer(
            string canonical_name,
            string localPath,
            string remoteFolderPath,
            string url,
            string user,
            string password,
            string repositoryId,
            string binding)
        {
            var credentials = new ServerCredentials() {
                Address = new Uri(url),
                Binding = binding,
                UserName = user,
                Password = password
            };

            var underTest = new ClientBrand(credentials, repositoryId, remoteFolderPath);

            Assert.That(underTest.TestServer(credentials), Is.True);
        }

        /// <summary>
        /// Test Client Brand
        /// </summary>
        [Test, TestCaseSource(typeof(ITUtils), "TestServers")]
        public void TestClientBrand(
            string canonical_name,
            string localPath,
            string remoteFolderPath,
            string url,
            string user,
            string password,
            string repositoryId,
            string binding)
        {
            var credentials = new ServerCredentials() {
                Address = new Uri(url),
                Binding = binding,
                UserName = user,
                Password = password
            };

            var underTest = new ClientBrand(credentials, repositoryId, remoteFolderPath);

            Assert.That(underTest.SetupServer(credentials), Is.True);

            foreach (string path in underTest.PathList) {
                DateTime date;
                Assert.That(underTest.GetFileDateTime(path, out date), Is.True);
                using (var stream = new MemoryStream()) {
                    Assert.That(underTest.GetFile(path, stream), Is.True);
                    Assert.That(stream.Length, Is.GreaterThan(0));
                }
            }
        }

        internal class ClientBrand : ClientBrandBase {
            private string repoName;
            private IRepository repository;
            private ISession session;
            private IFolder folder;

            private List<string> nameList = new List<string>() { "TestFile0", "TestFile1" };
            private List<string> pathList = new List<string>();

            public ClientBrand(ServerCredentials credentials, string repositoryId, string remoteFolderPath) {
                var parameters = CmisUtils.GetCmisParameters(credentials);
                var factory = SessionFactory.NewInstance();
                var repos = factory.GetRepositories(parameters);
                foreach (IRepository repo in repos) {
                    if (repo.Id == repositoryId) {
                        this.repository = repo;
                        this.repoName = repo.Name;
                    }
                }

                if (this.repository == null) {
                    throw new ArgumentException("No such repository for " + repositoryId);
                }

                this.session = this.repository.CreateSession();
                this.folder = this.session.GetObjectByPath(remoteFolderPath) as IFolder;
                if (this.folder == null) {
                    throw new ArgumentException("No such folder for " + remoteFolderPath);
                }

                foreach (string name in this.nameList) {
                    this.pathList.Add((remoteFolderPath + "/" + name).Replace("//", "/"));
                }

                this.DeleteFiles();
                this.CreateFiles();
            }

            ~ClientBrand() {
                this.DeleteFiles();
            }

            public override List<string> PathList {
                get {
                    return new List<string>(this.pathList);
                }
            }

            protected override string RepoName {
                get {
                    return this.repoName;
                }
            }

            private void DeleteFiles() {
                foreach (string path in this.pathList) {
                    try {
                        IDocument doc = this.session.GetObjectByPath(path) as IDocument;
                        doc.DeleteAllVersions();
                    } catch (Exception e) {
                        Logger.Debug("Failed to delete doc: ", e);
                    }
                }
            }

            private void CreateFiles() {
                foreach (string path in this.pathList) {
                    try {
                        this.folder.CreateDocument(Path.GetFileName(path), Encoding.UTF8.GetBytes(path), false);
                    } catch (Exception e) {
                        Logger.Debug("Failing to create doc: ", e);
                    }
                }
            }
        }
    }
}