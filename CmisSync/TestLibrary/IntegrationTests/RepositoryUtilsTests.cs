//-----------------------------------------------------------------------
// <copyright file="RepositoryUtilsTests.cs" company="GRAU DATA AG">
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

/**
 * Unit Tests for Repository Utils.
 * 
 * To use them, first create a JSON file containing the credentials/parameters to your CMIS server(s)
 * Put it in TestLibrary/test-servers.json and use this format:
[
    [
        "unittest1",
        "/mylocalpath",
        "/myremotepath",
        "http://example.com/p8cmis/resources/Service",
        "myuser",
        "mypassword",
        "repository987080"
    ],
    [
        "unittest2",
        "/mylocalpath",
        "/myremotepath",
        "http://example.org:8080/Nemaki/cmis",
        "myuser",
        "mypassword",
        "repo3"
    ]
]
 */

namespace TestLibrary.IntegrationTests {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Cmis.UiUtils;
    using CmisSync.Lib.Config;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl;
    using DotCMIS.Data.Impl;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class RepositoryUtilsTests : IsTestWithConfiguredLog4Net {
        [TestFixtureSetUp]
        public void ClassInit() {
            // Disable HTTPS Verification
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        [TearDown]
        public void TearDown() {
            // Reanable HTTPS Verification
            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        [Test, TestCaseSource(typeof(ITUtils), "TestServers"), Category("Slow"), Timeout(20000)]
        public void GetRepositories(
            string canonical_name,
            string localPath,
            string remoteFolderPath,
            string url,
            string user,
            string password,
            string repositoryId,
            string binding)
        {
            ServerCredentials credentials = new ServerCredentials() {
                Address = new Uri(url),
                Binding = binding,
                UserName = user,
                Password = password
            };

            var repos = credentials.GetRepositories();

            Assert.That(repos, Is.Not.Null.Or.Empty);

            foreach (var repo in repos) {
                Assert.That(string.IsNullOrEmpty(repo.Id), Is.False);
                Assert.That(string.IsNullOrEmpty(repo.Name), Is.False);
                Console.WriteLine(repo.ToString());
            }
        }
    }
}