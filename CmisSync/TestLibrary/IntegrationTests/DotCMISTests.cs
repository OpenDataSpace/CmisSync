//-----------------------------------------------------------------------
// <copyright file="DotCMISTests.cs" company="GRAU DATA AG">
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

namespace TestLibrary.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Text.RegularExpressions;

    using CmisSync.Lib;
    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Streams;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl;
    using DotCMIS.Data;
    using DotCMIS.Data.Impl;
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using TestUtils;

    /// <summary>
    /// Dot CMIS integration tests. Each method tests one specific test case. The test got to be finished after 15 mins, otherwise the test will fail.
    /// </summary>
    [TestFixture, Timeout(900000)]
    public class DotCMISTests : IsTestWithConfiguredLog4Net
    {
        /// <summary>
        /// Disable HTTPS Verification
        /// </summary>
        [TestFixtureSetUp]
        public void ClassInit()
        {
#if __MonoCS__
            Environment.SetEnvironmentVariable("MONO_XMLSERIALIZER_THS", "no");
#endif
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        /// <summary>
        /// Reanable HTTPS Verification
        /// </summary>
        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        /// <summary>
        /// Appends content stream to a cmis document, which is created.
        /// </summary>
        /// <param name='canonical_name'>
        /// Canonical_name.
        /// </param>
        /// <param name='localPath'>
        /// Local path.
        /// </param>
        /// <param name='remoteFolderPath'>
        /// Remote folder path.
        /// </param>
        /// <param name='url'>
        /// URL.
        /// </param>
        /// <param name='user'>
        /// User.
        /// </param>
        /// <param name='password'>
        /// Password.
        /// </param>
        /// <param name='repositoryId'>
        /// Repository identifier.
        /// </param>
        [Test, TestCaseSource(typeof(ITUtils), "TestServers"), Category("Slow")]
        public void AppendContentStreamTest(
            string canonical_name,
            string localPath,
            string remoteFolderPath,
            string url,
            string user,
            string password,
            string repositoryId)
        {
            ISession session = DotCMISSessionTests.CreateSession(user, password, url, repositoryId);

            IFolder folder = (IFolder)session.GetObjectByPath(remoteFolderPath);

            string filename = "testfile.txt";
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, filename);
            properties.Add(PropertyIds.ObjectTypeId, "cmis:document");
            IDocument doc = null;
            try {
                doc = session.GetObjectByPath(remoteFolderPath.TrimEnd('/') + "/" + filename) as IDocument;
                if (doc != null) {
                    doc.Delete(true);
                }
            } catch (Exception) {
            }
            string content = "test";
            doc = folder.CreateDocument(filename, content);
            Assert.That(doc.ContentStreamLength == content.Length, "returned document should have got content");
            for (int i = 0; i < 10; i++) {
                ContentStream contentStream = new ContentStream();
                contentStream.FileName = filename;
                contentStream.MimeType = MimeType.GetMIMEType(filename);
                contentStream.Length = content.Length;
                using (var memstream = new MemoryStream(Encoding.UTF8.GetBytes(content))) {
                    contentStream.Stream = memstream;
                    doc.AppendContentStream(contentStream, i == 9, true);
                }

                Assert.AreEqual(content.Length * (i + 2), doc.ContentStreamLength);
            }

            for (int i = 0; i < 10; i++) {
                ContentStream contentStream = new ContentStream();
                contentStream.FileName = filename;
                contentStream.MimeType = MimeType.GetMIMEType(filename);
                contentStream.Length = content.Length;
                using (var memstream = new MemoryStream(Encoding.UTF8.GetBytes(content))) {
                    contentStream.Stream = memstream;
                    doc.AppendContentStream(contentStream, true, true);
                }

                Assert.AreEqual(content.Length * (i + 2 + 10), doc.ContentStreamLength);
            }

            doc.DeleteAllVersions();
        }

        [Test, TestCaseSource(typeof(ITUtils), "TestServers"), Category("Slow")]
        public void RemovingRemoteFolderAndAddingADocumentToItShouldThrowException(
            string canonical_name,
            string localPath,
            string remoteFolderPath,
            string url,
            string user,
            string password,
            string repositoryId)
        {
            string subFolderName = "subFolder";
            ISession session = DotCMISSessionTests.CreateSession(user, password, url, repositoryId);
            try {
                IFolder dir = session.GetObjectByPath(remoteFolderPath.TrimEnd('/') + "/" + subFolderName) as IFolder;
                if (dir != null) {
                    dir.DeleteTree(true, null, true);
                }
            } catch (CmisObjectNotFoundException) {
            }
            IFolder folder = (IFolder)session.GetObjectByPath(remoteFolderPath);

            IFolder subFolder = folder.CreateFolder(subFolderName);

            IFolder subFolderInstanceCopy = (IFolder)session.GetObject(subFolder.Id);
            subFolder.DeleteTree(true, null, true);

            Assert.Throws<CmisObjectNotFoundException>(() => subFolderInstanceCopy.CreateDocument("testFile.bin", "testContent"));
        }

        /// <summary>
        /// Gets the root folder of repository.
        /// </summary>
        /// <param name='canonical_name'>
        /// Canonical_name.
        /// </param>
        /// <param name='localPath'>
        /// Local path.
        /// </param>
        /// <param name='remoteFolderPath'>
        /// Remote folder path.
        /// </param>
        /// <param name='url'>
        /// URL.
        /// </param>
        /// <param name='user'>
        /// User.
        /// </param>
        /// <param name='password'>
        /// Password.
        /// </param>
        /// <param name='repositoryId'>
        /// Repository identifier.
        /// </param>
        [Test, TestCaseSource(typeof(ITUtils), "TestServers"), Category("Slow")]
        public void GetRootFolderOfRepository(
            string canonical_name,
            string localPath,
            string remoteFolderPath,
            string url,
            string user,
            string password,
            string repositoryId)
        {
            ISession session = DotCMISSessionTests.CreateSession(user, password, url, repositoryId);
            IFolder remoteFolder = (IFolder)session.GetObject(repositoryId);
            Assert.IsNotNull(remoteFolder);
        }

        /// <summary>
        /// Tests the sync property on a cmis document object.
        /// </summary>
        /// <param name='canonical_name'>
        /// Canonical_name.
        /// </param>
        /// <param name='localPath'>
        /// Local path.
        /// </param>
        /// <param name='remoteFolderPath'>
        /// Remote folder path.
        /// </param>
        /// <param name='url'>
        /// URL.
        /// </param>
        /// <param name='user'>
        /// User.
        /// </param>
        /// <param name='password'>
        /// Password.
        /// </param>
        /// <param name='repositoryId'>
        /// Repository identifier.
        /// </param>
        [Test, TestCaseSource(typeof(ITUtils), "TestServers"), Category("Slow")]
        public void GetSyncPropertyFromFile(
            string canonical_name,
            string localPath,
            string remoteFolderPath,
            string url,
            string user,
            string password,
            string repositoryId)
        {
            ISession session = DotCMISSessionTests.CreateSession(user, password, url, repositoryId);
            IFolder folder = (IFolder)session.GetObjectByPath(remoteFolderPath);
            string filename = "testfile.txt";
            try {
                IDocument doc = session.GetObjectByPath(remoteFolderPath.TrimEnd('/') + "/" + filename) as IDocument;
                if (doc != null) {
                    doc.Delete(true);
                }
            } catch (CmisObjectNotFoundException) {
            }

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, filename);
            properties.Add(PropertyIds.ObjectTypeId, "cmis:document");
            IList<string> devices = new List<string>();
            devices.Add("*");
            properties.Add("gds:ignoreDeviceIds", devices);
            IList<string> ids = new List<string>();
            ids.Add("gds:sync");
            properties.Add(PropertyIds.SecondaryObjectTypeIds, ids);

            IDocument emptyDoc = folder.CreateDocument(properties, null, null);
            Assert.That(emptyDoc.ContentStreamLength == 0 || emptyDoc.ContentStreamLength == null);
            var context = new OperationContext();
            IDocument requestedDoc = session.GetObject(emptyDoc, context) as IDocument;
            bool propertyFound = false;
            foreach (var prop in requestedDoc.Properties)
            {
                if (prop.Id == "gds:ignoreDeviceIds")
                {
                    propertyFound = true;
                    Assert.AreEqual("*", prop.FirstValue as string);
                }
            }

            Assert.IsTrue(propertyFound);
            emptyDoc.DeleteAllVersions();
        }

        /*
        [Ignore]
        [Test, Category("Fast")]
        public void RegexTestForRemoteHashProperty() {
            Regex entryRegex = new Regex(@"^\{.+\}[0-9a-fA-F]+$");
            Assert.That(entryRegex.IsMatch("{sha-1}2fd4e1c67a2d28fced849ee1bb76e7391b93eb1233f80f8a"), Is.True);
            Assert.That(entryRegex.IsMatch("{sha-256}e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"), Is.True);
            Assert.That(entryRegex.IsMatch("{}1234567"), Is.False);
            Assert.That(entryRegex.IsMatch("sha-1}1234567"), Is.False);
            Assert.That(entryRegex.IsMatch("{sadf24er35}"), Is.False);
        } */

        [Test, TestCaseSource(typeof(ITUtils), "TestServers"), Category("Slow")]
        public void GetContentStreamHash(
            string canonical_name,
            string localPath,
            string remoteFolderPath,
            string url,
            string user,
            string password,
            string repositoryId) {
            Regex entryRegex = new Regex(@"^\{.+\}[0-9a-fA-F]+$");
            ISession session = DotCMISSessionTests.CreateSession(user, password, url, repositoryId);
            IFolder folder = (IFolder)session.GetObjectByPath(remoteFolderPath);
            string filename = "hashedfile.txt";
            try {
                IDocument doc = session.GetObjectByPath(remoteFolderPath.TrimEnd('/') + "/" + filename) as IDocument;
                if (doc != null) {
                    doc.Delete(true);
                }
            } catch (CmisObjectNotFoundException) {
            }

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, filename);
            properties.Add(PropertyIds.ObjectTypeId, "cmis:document");
            using (var oneByteStream = new MemoryStream(new byte[1])) {
                ContentStream contentStream = new ContentStream();
                contentStream.MimeType = MimeType.GetMIMEType(filename);
                contentStream.Length = 1;
                contentStream.Stream = oneByteStream;
                var emptyDoc = folder.CreateDocument(properties, contentStream, null);
                var context = new OperationContext();
                IDocument requestedDoc = session.GetObject(emptyDoc, context) as IDocument;
                foreach (var prop in requestedDoc.Properties) {
                    if (prop.Id == "cmis:contentStreamHash") {
                        Assert.That(prop.IsMultiValued, Is.True);
                        if (prop.Values != null) {
                            foreach (string entry in prop.Values) {
                                Assert.That(entryRegex.IsMatch(entry));
                            }
                        }
                    }
                }
                byte[] remoteHash = requestedDoc.ContentStreamHash();
                if (remoteHash != null) {
                    Assert.That(remoteHash, Is.EqualTo(SHA1.Create().ComputeHash(new byte[1])));
                }

                requestedDoc.Delete(true);
            }
        }

        /// <summary>
        /// Sets the content stream on an empty file.
        /// The file will be created as first step and after success, the content will be set.
        /// </summary>
        /// <param name='canonical_name'>
        /// Canonical_name.
        /// </param>
        /// <param name='localPath'>
        /// Local path.
        /// </param>
        /// <param name='remoteFolderPath'>
        /// Remote folder path.
        /// </param>
        /// <param name='url'>
        /// URL.
        /// </param>
        /// <param name='user'>
        /// User.
        /// </param>
        /// <param name='password'>
        /// Password.
        /// </param>
        /// <param name='repositoryId'>
        /// Repository identifier.
        /// </param>
        [Test, TestCaseSource(typeof(ITUtils), "TestServers"), Category("Slow")]
        public void SetContentStreamTest(
            string canonical_name,
            string localPath,
            string remoteFolderPath,
            string url,
            string user,
            string password,
            string repositoryId)
        {
            ISession session = DotCMISSessionTests.CreateSession(user, password, url, repositoryId);

            IFolder folder = (IFolder)session.GetObjectByPath(remoteFolderPath);

            string filename = "testfile.txt";
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, filename);
            properties.Add(PropertyIds.ObjectTypeId, "cmis:document");
            try {
                IDocument doc = session.GetObjectByPath(remoteFolderPath.TrimEnd('/') + "/" + filename) as IDocument;
                if (doc != null) {
                    doc.Delete(true);
                }
            } catch (CmisObjectNotFoundException)
            {
            }

            IDocument emptyDoc = folder.CreateDocument(properties, null, null);

            Assert.That(emptyDoc.ContentStreamLength == null || emptyDoc.ContentStreamLength == 0, "returned document shouldn't got any content");
            string content = string.Empty;
            content += "Test ";
            ContentStream contentStream = new ContentStream();
            contentStream.FileName = filename;
            contentStream.MimeType = MimeType.GetMIMEType(filename);
            contentStream.Length = content.Length;
            using (var memstream = new MemoryStream(Encoding.UTF8.GetBytes(content))) {
                contentStream.Stream = memstream;
                emptyDoc.SetContentStream(contentStream, true, true);
            }

            Assert.AreEqual(content.Length, emptyDoc.ContentStreamLength);
            emptyDoc.DeleteContentStream(true);
            content += "Test ";
            contentStream = new ContentStream();
            contentStream.FileName = filename;
            contentStream.MimeType = MimeType.GetMIMEType(filename);
            contentStream.Length = content.Length;
            using (var memstream = new MemoryStream(Encoding.UTF8.GetBytes(content))) {
                contentStream.Stream = memstream;
                emptyDoc.SetContentStream(contentStream, true, true);
            }

            Assert.AreEqual(content.Length, emptyDoc.ContentStreamLength);
            emptyDoc.DeleteContentStream(true);
            content += "Test ";
            contentStream = new ContentStream();
            contentStream.FileName = filename;
            contentStream.MimeType = MimeType.GetMIMEType(filename);
            contentStream.Length = content.Length;
            using (var memstream = new MemoryStream(Encoding.UTF8.GetBytes(content))) {
                contentStream.Stream = memstream;
                emptyDoc.SetContentStream(contentStream, true, true);
            }

            Assert.AreEqual(content.Length, emptyDoc.ContentStreamLength);
            emptyDoc.DeleteAllVersions();
        }

        [Test, TestCaseSource(typeof(ITUtils), "TestServers"), Category("Slow")]
        public void CreateDocumentWithContentStream(
            string canonical_name,
            string localPath,
            string remoteFolderPath,
            string url,
            string user,
            string password,
            string repositoryId) {
            ISession session = DotCMISSessionTests.CreateSession(user, password, url, repositoryId);

            IFolder folder = (IFolder)session.GetObjectByPath(remoteFolderPath);
            string content = "content";
            IDocument doc = folder.CreateDocument("name", content);

            Assert.That(doc.ContentStreamLength, Is.EqualTo(content.Length));
            doc.DeleteAllVersions();
        }

        [Test, TestCaseSource(typeof(ITUtils), "TestServers"), Category("Slow")]
        public void CreateDocumentWithCreationAndModificationDate(
            string canonical_name,
            string localPath,
            string remoteFolderPath,
            string url,
            string user,
            string password,
            string repositoryId) {
            ISession session = DotCMISSessionTests.CreateSession(user, password, url, repositoryId);
            if (!session.IsServerAbleToUpdateModificationDate()) {
                Assert.Ignore("Server is not able to sync modification dates");
            }
            string filename = "name";
            IDocument doc;
            try {
                doc = session.GetObjectByPath(remoteFolderPath.TrimEnd('/') + "/" + filename) as IDocument;
                if (doc != null) {
                    doc.Delete(true);
                }
            } catch (CmisObjectNotFoundException)
            {
            }

            DateTime creationDate = DateTime.UtcNow - TimeSpan.FromDays(1);
            DateTime modificationDate = DateTime.UtcNow - TimeSpan.FromHours(1);
            IFolder folder = (IFolder)session.GetObjectByPath(remoteFolderPath);

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.ObjectTypeId, "cmis:document");
            properties.Add(PropertyIds.Name, filename);
            properties.Add(PropertyIds.CreationDate, creationDate);
            properties.Add(PropertyIds.LastModificationDate, modificationDate);

            doc = folder.CreateDocument(properties, null, null);

            Assert.That(((DateTime)doc.LastModificationDate - modificationDate).Seconds, Is.EqualTo(0), "Wrong modification date");
            Assert.That(((DateTime)doc.CreationDate - creationDate).Seconds, Is.EqualTo(0), "Wrong creation date");
            doc.DeleteAllVersions();
        }

        [Test, TestCaseSource(typeof(ITUtils), "TestServers"), Category("Slow")]
        public void EnsureFileNameStaysEqualWhileUploading(
            string canonical_name,
            string localPath,
            string remoteFolderPath,
            string url,
            string user,
            string password,
            string repositoryId) {
            ISession session = DotCMISSessionTests.CreateSession(user, password, url, repositoryId);

            IFolder folder = (IFolder)session.GetObjectByPath(remoteFolderPath);

            string filename = "testfile.txt";
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, filename);
            properties.Add(PropertyIds.ObjectTypeId, "cmis:document");
            try {
                IDocument doc = session.GetObjectByPath(remoteFolderPath.TrimEnd('/') + "/" + filename) as IDocument;
                if (doc != null) {
                    doc.Delete(true);
                }
            } catch (CmisObjectNotFoundException)
            {
            }

            IDocument emptyDoc = folder.CreateDocument(properties, null, null);
            int length = 1024 * 1024;
            byte[] content = new byte[length];
            ContentStream contentStream = new ContentStream();
            contentStream.FileName = filename;
            contentStream.MimeType = MimeType.GetMIMEType(filename);
            contentStream.Length = content.Length;
            Action assert = delegate {
                Assert.That((session.GetObject(emptyDoc.Id, OperationContextFactory.CreateNonCachingPathIncludingContext(session)) as IDocument).Name, Is.EqualTo(filename));
            };

            using (var memstream = new AssertingStream(new MemoryStream(content), assert)) {
                contentStream.Stream = memstream;
                emptyDoc.SetContentStream(contentStream, true, true);
            }
        }

        [Test, TestCaseSource(typeof(ITUtils), "TestServers"), Category("Slow")]
        public void SetJPEGContentStream(
            string canonical_name,
            string localPath,
            string remoteFolderPath,
            string url,
            string user,
            string password,
            string repositoryId)
        {
            ISession session = DotCMISSessionTests.CreateSession(user, password, url, repositoryId);
            IFolder folder = (IFolder)session.GetObjectByPath(remoteFolderPath);
            string filename = "testfile.jpg";
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, filename);
            properties.Add(PropertyIds.ObjectTypeId, "cmis:document");
            try {
                IDocument doc = session.GetObjectByPath(remoteFolderPath.TrimEnd('/') + "/" + filename) as IDocument;
                if (doc != null) {
                    doc.Delete(true);
                }
            }
            catch (CmisObjectNotFoundException) {
            }

            IDocument emptyDoc = folder.CreateDocument(properties, null, null);
            Assert.That(emptyDoc.ContentStreamLength == null || emptyDoc.ContentStreamLength == 0, "returned document shouldn't got any content");
            int contentLength = 1024;
            byte[] content = new byte[contentLength];
            using (RandomNumberGenerator random = RandomNumberGenerator.Create()) {
                random.GetBytes(content);
            }

            ContentStream contentStream = new ContentStream();
            contentStream.FileName = filename;
            contentStream.MimeType = MimeType.GetMIMEType(filename);
            contentStream.Length = content.Length;

            using (var memstream = new MemoryStream(content)) {
                contentStream.Stream = memstream;
                emptyDoc.SetContentStream(contentStream, true, true);
            }

            Assert.AreEqual(content.Length, emptyDoc.ContentStreamLength, "Setting content failed");
            IDocument randomDoc = session.GetObjectByPath(emptyDoc.Paths[0]) as IDocument;
            Assert.That(randomDoc != null);
            Assert.AreEqual(content.Length, randomDoc.ContentStreamLength, "Getting content on new object failed");
            emptyDoc.DeleteAllVersions();
        }

        [Test, TestCaseSource(typeof(ITUtils), "TestServers"), Category("Slow")]
        public void RenameFolder(
            string canonical_name,
            string localPath,
            string remoteFolderPath,
            string url,
            string user,
            string password,
            string repositoryId)
        {
            ISession session = DotCMISSessionTests.CreateSession(user, password, url, repositoryId);
            IFolder rootFolder = (IFolder)session.GetObjectByPath(remoteFolderPath);
            string folderName = "1";
            string newFolderName = "was 1 in past";
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, folderName);
            properties.Add(PropertyIds.ObjectTypeId, "cmis:folder");
            try {
                IFolder folder = session.GetObjectByPath(remoteFolderPath.TrimEnd('/') + "/" + folderName) as IFolder;
                if (folder != null) {
                    folder.Delete(true);
                }

                folder = session.GetObjectByPath(remoteFolderPath.TrimEnd('/') + "/" + newFolderName) as IFolder;
                if (folder != null) {
                    folder.Delete(true);
                }
            }
            catch (CmisObjectNotFoundException) {
            }

            IFolder newFolder = rootFolder.CreateFolder(properties);
            newFolder.Rename(newFolderName, true);
            Assert.That(newFolder.Name, Is.EqualTo(newFolderName));
            newFolder.Delete(true);
        }

        [Test, TestCaseSource(typeof(ITUtils), "TestServers"), Category("Slow")]
        public void RenameRemoteFolderChangesChangeLogToken(
            string canonical_name,
            string localPath,
            string remoteFolderPath,
            string url,
            string user,
            string password,
            string repositoryId)
        {
            ISession session = DotCMISSessionTests.CreateSession(user, password, url, repositoryId);
            IFolder rootFolder = (IFolder)session.GetObjectByPath(remoteFolderPath);
            string folderName = "1";
            string newFolderName = "2";
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, folderName);
            properties.Add(PropertyIds.ObjectTypeId, "cmis:folder");
            try {
                IFolder folder = session.GetObjectByPath(remoteFolderPath.TrimEnd('/') + "/" + folderName) as IFolder;
                if (folder != null) {
                    folder.Delete(true);
                }

                folder = session.GetObjectByPath(remoteFolderPath.TrimEnd('/') + "/" + newFolderName) as IFolder;
                if (folder != null) {
                    folder.Delete(true);
                }
            }
            catch (CmisObjectNotFoundException) {
            }

            IFolder newFolder = rootFolder.CreateFolder(properties);
            string changeLogToken = session.RepositoryInfo.LatestChangeLogToken;
            string changeToken = newFolder.ChangeToken;
            newFolder.Rename(newFolderName, true);

            Assert.That(newFolder.ChangeToken, Is.Not.EqualTo(changeToken));

            newFolder.Delete(true);
        }

        [Test, TestCaseSource(typeof(ITUtils), "TestServers"), Category("Slow"), Category("Erratic"), Ignore("Doesn't happend anymore")]
        public void GetChildrenDoesNotProducesServerProtocolViolationException(
            string canonical_name,
            string localPath,
            string remoteFolderPath,
            string url,
            string user,
            string password,
            string repositoryId) {
            ISession session = DotCMISSessionTests.CreateSession(user, password, url, repositoryId);
            for (int i = 0; i < 1000; i++) {
                IFolder root = (IFolder)session.GetObjectByPath(remoteFolderPath);
                foreach (var child in root.GetChildren()) {
                    Console.WriteLine(child.Name);
                }
            }
        }

        private class AssertingStream : StreamWrapper {
            private Action verification;

            public AssertingStream(Stream stream, Action verification) : base(stream) {
                this.verification = verification;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                this.verification();
                return base.Read(buffer, offset, count);
            }
        }
    }

    /// <summary>
    /// Dot CMIS session tests. Each log in process must be able to be executed in 60 seconds, otherwise the tests will fail.
    /// </summary>
    [TestFixture, Timeout(60000)]
    public class DotCMISSessionTests
    {
        /// <summary>
        /// Creates a cmis Atom Pub session with the given credentials.
        /// </summary>
        /// <returns>
        /// The session.
        /// </returns>
        /// <param name='user'>
        /// User.
        /// </param>
        /// <param name='password'>
        /// Password.
        /// </param>
        /// <param name='url'>
        /// URL.
        /// </param>
        /// <param name='repoId'>
        /// Repo identifier.
        /// </param>
        public static ISession CreateSession(
            string user,
            Password password,
            string url,
            string repoId)
        {
            Dictionary<string, string> cmisParameters = new Dictionary<string, string>();
            cmisParameters[SessionParameter.BindingType] = BindingType.AtomPub;
            cmisParameters[SessionParameter.AtomPubUrl] = url;
            cmisParameters[SessionParameter.User] = user;
            cmisParameters[SessionParameter.Password] = password.ToString();
            cmisParameters[SessionParameter.RepositoryId] = repoId;

            // Sets the Connect Timeout to 60 secs
            cmisParameters[SessionParameter.ConnectTimeout] = "60000";

            ISession session = SessionFactory.NewInstance().CreateSession(cmisParameters);
            HashSet<string> filters = new HashSet<string>();
            filters.Add("cmis:objectId");
            filters.Add("cmis:name");
            filters.Add("cmis:contentStreamFileName");
            filters.Add("cmis:contentStreamLength");
            filters.Add("cmis:lastModificationDate");
            filters.Add("cmis:creationDate");
            filters.Add("cmis:path");
            filters.Add("cmis:changeToken");
            filters.Add(PropertyIds.SecondaryObjectTypeIds);
            HashSet<string> renditions = new HashSet<string>();
            renditions.Add("cmis:none");
            session.DefaultContext = session.CreateOperationContext(filters, false, true, false, IncludeRelationshipsFlag.None, null, true, null, true, 100);
            return session;
        }

        /// <summary>
        /// Accept every SSL Server if an invalid cert is returned
        /// </summary>
        [TestFixtureSetUp]
        public void ClassInit()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        /// <summary>
        /// Revoke the acceptance of invalid SSL certificates.
        /// </summary>
        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        /// <summary>
        /// Tests to creates a basic session.
        /// </summary>
        /// <param name='canonical_name'>
        /// Canonical_name.
        /// </param>
        /// <param name='localPath'>
        /// Local path.
        /// </param>
        /// <param name='remoteFolderPath'>
        /// Remote folder path.
        /// </param>
        /// <param name='url'>
        /// URL.
        /// </param>
        /// <param name='user'>
        /// User.
        /// </param>
        /// <param name='password'>
        /// Password.
        /// </param>
        /// <param name='repositoryId'>
        /// Repository identifier.
        /// </param>
        [Test, TestCaseSource(typeof(ITUtils), "TestServers"), Category("Slow")]
        public void CreateSession(
            string canonical_name,
            string localPath,
            string remoteFolderPath,
            string url,
            string user,
            string password,
            string repositoryId)
        {
            CreateSession(user, password, url, repositoryId);
        }

        /// <summary>
        /// Creates the session with device management and user agent.
        /// </summary>
        /// <param name='canonical_name'>
        /// Canonical_name.
        /// </param>
        /// <param name='localPath'>
        /// Local path.
        /// </param>
        /// <param name='remoteFolderPath'>
        /// Remote folder path.
        /// </param>
        /// <param name='url'>
        /// URL.
        /// </param>
        /// <param name='user'>
        /// User.
        /// </param>
        /// <param name='password'>
        /// Password.
        /// </param>
        /// <param name='repositoryId'>
        /// Repository identifier.
        /// </param>
        [Test, TestCaseSource(typeof(ITUtils), "TestServers"), Category("Slow")]
        public void CreateSessionWithDeviceManagementAndUserAgent(
            string canonical_name,
            string localPath,
            string remoteFolderPath,
            string url,
            string user,
            string password,
            string repositoryId)
        {
            Dictionary<string, string> cmisParameters = new Dictionary<string, string>();
            cmisParameters[SessionParameter.BindingType] = BindingType.AtomPub;
            cmisParameters[SessionParameter.AtomPubUrl] = url.ToString();
            cmisParameters[SessionParameter.User] = user;
            cmisParameters[SessionParameter.Password] = password;
            cmisParameters[SessionParameter.RepositoryId] = repositoryId;

            // Sets the Connect Timeout to 60 secs
            cmisParameters[SessionParameter.ConnectTimeout] = "60000";

            // Sets the Read Timeout to 60 secs
            cmisParameters[SessionParameter.ReadTimeout] = "60000";
            cmisParameters[SessionParameter.DeviceIdentifier] = ConfigManager.CurrentConfig.DeviceId.ToString();
            cmisParameters[SessionParameter.UserAgent] = Utils.CreateUserAgent();
            SessionFactory.NewInstance().CreateSession(cmisParameters);
        }

        /// <summary>
        /// Creates the session with compression enabled.
        /// </summary>
        /// <param name='canonical_name'>
        /// Canonical_name.
        /// </param>
        /// <param name='localPath'>
        /// Local path.
        /// </param>
        /// <param name='remoteFolderPath'>
        /// Remote folder path.
        /// </param>
        /// <param name='url'>
        /// URL.
        /// </param>
        /// <param name='user'>
        /// User.
        /// </param>
        /// <param name='password'>
        /// Password.
        /// </param>
        /// <param name='repositoryId'>
        /// Repository identifier.
        /// </param>
        [Test, TestCaseSource(typeof(ITUtils), "TestServers"), Category("Slow")]
        public void CreateSessionWithCompressionEnabled(
            string canonical_name,
            string localPath,
            string remoteFolderPath,
            string url,
            string user,
            string password,
            string repositoryId)
        {
            Dictionary<string, string> cmisParameters = new Dictionary<string, string>();
            cmisParameters[SessionParameter.BindingType] = BindingType.AtomPub;
            cmisParameters[SessionParameter.AtomPubUrl] = url.ToString();
            cmisParameters[SessionParameter.User] = user;
            cmisParameters[SessionParameter.Password] = password;
            cmisParameters[SessionParameter.RepositoryId] = repositoryId;
            cmisParameters[SessionParameter.Compression] = bool.TrueString;

            // Sets the Connect Timeout to 60 secs
            cmisParameters[SessionParameter.ConnectTimeout] = "60000";

            // Sets the Read Timeout to 60 secs
            cmisParameters[SessionParameter.ReadTimeout] = "60000";
            SessionFactory.NewInstance().CreateSession(cmisParameters);
        }
    }
}
