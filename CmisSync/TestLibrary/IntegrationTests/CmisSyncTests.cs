//-----------------------------------------------------------------------
// <copyright file="CmisSyncTests.cs" company="GRAU DATA AG">
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
 * Unit Tests for CmisSync.
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

namespace TestLibrary.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using CmisSync.Lib;
    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Sync;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl;
    using DotCMIS.Data.Impl;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    // Default timeout per test is 15 minutes
    [TestFixture, Timeout(900000)]
    public class CmisSyncTests
    {
        private readonly string cmisSyncDir = ConfigManager.CurrentConfig.GetFoldersPath();
        private readonly int heavyNumber = 10;
        private readonly int heavyFileSize = 1024;

        /// <summary>
        /// Waits until checkStop is true or waiting duration is reached.
        /// </summary>
        /// <returns>
        /// True if checkStop is true, otherwise waits for pollInterval miliseconds and checks again until the wait threshold is reached.
        /// </returns>
        /// <param name='checkStop'>
        /// Checks if the condition, which is waited for is <c>true</c>.
        /// </param>
        /// <param name='wait'>
        /// Waiting threshold. If this is reached, <c>false</c> will be returned.
        /// </param>
        /// <param name='pollInterval'>
        /// Sleep duration between two condition validations by calling checkStop.
        /// </param>
        public static bool WaitUntilDone(Func<bool> checkStop, int wait = 300000, int pollInterval = 1000)
        {
            while (wait > 0)
            {
                System.Threading.Thread.Sleep(pollInterval);
                wait -= pollInterval;
                if (checkStop()) {
                    return true;
                }

                Console.WriteLine(string.Format("Retry Wait in {0}ms", pollInterval));
            }

            Console.WriteLine("Wait was not successful");
            return false;
        }

        [TestFixtureSetUp]
        public void ClassInit()
        {
            // Disable HTTPS Verification
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            try {
                File.Delete(ConfigManager.CurrentConfig.GetLogFilePath());
            } catch (IOException) {
            }

            log4net.Config.XmlConfigurator.Configure(ConfigManager.CurrentConfig.GetLog4NetConfig());
        }

        [TearDown]
        public void TearDown()
        {
            foreach (string file in Directory.GetFiles(this.cmisSyncDir)) {
                if (file.EndsWith(".cmissync"))
                {
                    File.Delete(file);
                }
            }

            // Reanable HTTPS Verification
            ServicePointManager.ServerCertificateValidationCallback = null;
        }



        public IDocument CopyDocument(IFolder folder, IDocument source, string name)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, name);
            properties.Add(PropertyIds.ObjectTypeId, "cmis:document");

            return folder.CreateDocumentFromSource(source, properties, null);
        }

        public IDocument RenameDocument(IDocument source, string name)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, name);

            return (IDocument)source.UpdateProperties(properties);
        }

        public IFolder RenameFolder(IFolder source, string name)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, name);

            return (IFolder)source.UpdateProperties(properties);
        }

        public void CreateHeavyFolder(string root)
        {
            for (int folderNumber = 0; folderNumber < this.heavyNumber; ++folderNumber)
            {
                string folder = Path.Combine(root, folderNumber.ToString());
                Directory.CreateDirectory(folder);
                for (int fileNumber = 0; fileNumber < this.heavyNumber; ++fileNumber)
                {
                    string file = Path.Combine(folder, fileNumber.ToString());
                    using (Stream stream = File.OpenWrite(file))
                    {
                        byte[] content = new byte[this.heavyFileSize];
                        for (int i = 0; i < this.heavyFileSize; ++i)
                        {
                            content[i] = (byte)('A' + (fileNumber % 10));
                        }

                        stream.Write(content, 0, content.Length);
                    }
                }
            }
        }

        public bool CheckHeavyFolder(string root)
        {
            for (int folderNumber = 0; folderNumber < this.heavyNumber; ++folderNumber)
            {
                string folder = Path.Combine(root, folderNumber.ToString());
                if (!Directory.Exists(folder))
                {
                    return false;
                }

                for (int fileNumber = 0; fileNumber < this.heavyNumber; ++fileNumber)
                {
                    string file = Path.Combine(folder, fileNumber.ToString());
                    FileInfo info = new FileInfo(file);
                    if (!info.Exists || info.Length != this.heavyFileSize)
                    {
                        return false;
                    }

                    try
                    {
                        using (Stream stream = File.OpenRead(file))
                        {
                            byte[] content = new byte[this.heavyFileSize];
                            stream.Read(content, 0, this.heavyFileSize);
                            for (int i = 0; i < this.heavyFileSize; ++i)
                            {
                                if (content[i] != (byte)('A' + (fileNumber % 10)))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void CreateHeavyFolderRemote(IFolder root)
        {
            for (int folderNumber = 0; folderNumber < this.heavyNumber; ++folderNumber)
            {
                IFolder folder = root.CreateFolder(folderNumber.ToString());
                for (int fileNumber = 0; fileNumber < this.heavyNumber; ++fileNumber)
                {
                    string content = new string((char)('A' + (fileNumber % 10)), this.heavyFileSize);
                    folder.CreateDocument(fileNumber.ToString(), content);
                }
            }
        }

        /* /////////////////////////// TESTS /////////////////////////// */

        [Test, TestCaseSource(typeof(ITUtils), "TestServers"), Category("Slow"), Timeout(20000)]
        public void GetRepositories(
            string canonical_name,
            string localPath,
            string remoteFolderPath,
            string url,
            string user,
            string password,
            string repositoryId)
        {
            CmisSync.Lib.Credentials.ServerCredentials credentials = new CmisSync.Lib.Credentials.ServerCredentials()
            {
                Address = new Uri(url),
                UserName = user,
                Password = password
            };

            Dictionary<string, string> repos = CmisUtils.GetRepositories(credentials);

            foreach (KeyValuePair<string, string> pair in repos)
            {
                Console.WriteLine(pair.Key + " : " + pair.Value);
            }

            Assert.NotNull(repos);
        }

        // Write a file and immediately check whether it has been created.
        // Should help to find out whether CMIS servers are synchronous or not.
        [Test, TestCaseSource(typeof(ITUtils), "TestServers"), Category("Slow")]
        public void WriteThenRead(
            string canonical_name,
            string localPath,
            string remoteFolderPath,
            string url,
            string user,
            string password,
            string repositoryId)
        {
            string fileName = "test.txt";
            var cmisParameters = new Dictionary<string, string>();
            cmisParameters[SessionParameter.BindingType] = BindingType.AtomPub;
            cmisParameters[SessionParameter.AtomPubUrl] = url;
            cmisParameters[SessionParameter.User] = user;
            cmisParameters[SessionParameter.Password] = password;
            cmisParameters[SessionParameter.RepositoryId] = repositoryId;

            SessionFactory factory = SessionFactory.NewInstance();
            ISession session = factory.CreateSession(cmisParameters);

            // IFolder root = session.GetRootFolder();
            IFolder root = (IFolder)session.GetObjectByPath(remoteFolderPath);

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, fileName);
            properties.Add(PropertyIds.ObjectTypeId, "cmis:document");

            ContentStream contentStream = new ContentStream();
            contentStream.FileName = fileName;
            contentStream.MimeType = MimeType.GetMIMEType(fileName); // Should CmisSync try to guess?
            byte[] bytes = Encoding.UTF8.GetBytes("Hello,world!");
            contentStream.Stream = new MemoryStream(bytes);
            contentStream.Length = bytes.Length;

            // Create file.
            DotCMIS.Enums.VersioningState? state = null;
            if (true != session.RepositoryInfo.Capabilities.IsAllVersionsSearchableSupported)
            {
                state = DotCMIS.Enums.VersioningState.None;
            }

            session.CreateDocument(properties, root, contentStream, state);

            // Check whether file is present.
            IItemEnumerable<ICmisObject> children = root.GetChildren();
            bool found = false;
            foreach (ICmisObject child in children)
            {
                string childFileName = (string)child.GetPropertyValue(PropertyIds.Name);
                Console.WriteLine(childFileName);
                if (childFileName.Equals(fileName))
                {
                    found = true;
                }
            }

            Assert.True(found);

            // Clean.
            IDocument doc = (IDocument)session.GetObjectByPath((remoteFolderPath + "/" + fileName).Replace("//", "/"));
            doc.DeleteAllVersions();
        }

        [Test, TestCaseSource(typeof(ITUtils), "TestServersFuzzy"), Category("Slow"), Timeout(60000)]
        public void GetRepositoriesFuzzy(string url, string user, string password)
        {
            CmisSync.Lib.Credentials.ServerCredentials credentials = new CmisSync.Lib.Credentials.ServerCredentials()
            {
                Address = new Uri(url),
                UserName = user,
                Password = password
            };
            Tuple<CmisServer, Exception> server = CmisUtils.GetRepositoriesFuzzy(credentials);
            Assert.NotNull(server.Item1);
        }

        /* /////////////////////////// HELPER METHODS /////////////////////////// */

        private void DeleteDirectoryIfExists(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        private void CleanDirectory(string path)
        {
            // Delete recursively.
            this.DeleteDirectoryIfExists(path);

            // Delete database.
            string database = path + ".cmissync";
            if (File.Exists(database))
            {
                try
                {
                    File.Delete(database);
                }
                catch (IOException ex)
                {
                    Console.WriteLine("Exception on testing side, ignoring " + database + ":" + ex);
                }
            }

            // Prepare empty directory.
            Directory.CreateDirectory(path);
        }

        private void CleanAll(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);

            try
            {
                // Delete all local files/folders.
                foreach (FileInfo file in directory.GetFiles())
                {
                    if (file.Name.EndsWith(".sync"))
                    {
                        continue;
                    }

                    try
                    {
                        file.Delete();
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine("Exception on testing side, ignoring " + file.FullName + ":" + ex);
                    }
                }

                foreach (DirectoryInfo dir in directory.GetDirectories())
                {
                    this.CleanAll(dir.FullName);

                    try
                    {
                        dir.Delete();
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine("Exception on testing side, ignoring " + dir.FullName + ":" + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception on testing side, ignoring " + ex);
            }
        }

        private ISession CreateSession(RepoInfo repoInfo)
        {
            Dictionary<string, string> cmisParameters = new Dictionary<string, string>();
            cmisParameters[SessionParameter.BindingType] = BindingType.AtomPub;
            cmisParameters[SessionParameter.AtomPubUrl] = repoInfo.Address.ToString();
            cmisParameters[SessionParameter.User] = repoInfo.User;
            cmisParameters[SessionParameter.Password] = repoInfo.GetPassword().ToString();
            cmisParameters[SessionParameter.RepositoryId] = repoInfo.RepositoryId;
            cmisParameters[SessionParameter.ConnectTimeout] = "-1";

            return SessionFactory.NewInstance().CreateSession(cmisParameters);
        }

        private string ListAllFiles(string folder) {
            StringBuilder builder = new StringBuilder();
            foreach (string f in Directory.GetFiles(folder)) {
                builder.Append(f);
                builder.Append('\t');
                builder.Append(new FileInfo(f).Length);
                builder.Append(Environment.NewLine);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Creates or modifies binary file.
        /// </summary>
        /// <returns>
        /// Path of the created or modified binary file
        /// </returns>
        /// <param name='file'>
        /// File path
        /// </param>
        /// <param name='length'>
        /// Length (default is 1024)
        /// </param>
        private string CreateOrModifyBinaryFile(string file, int length = 1024)
        {
            using (Stream stream = File.Open(file, FileMode.Create))
            {
                byte[] content = new byte[length];
                stream.Write(content, 0, content.Length);
            }

            return file;
        }
    }
}
