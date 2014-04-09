using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Net;
using System.IO;
using System.Text;
//using System.Diagnostics;

using CmisSync.Lib;
using CmisSync.Lib.Cmis;
using CmisSync.Lib.Sync;
using CmisSync.Lib.Credentials;

using DotCMIS;
using DotCMIS.Client;
using DotCMIS.Client.Impl;
using DotCMIS.Data;
using DotCMIS.Data.Impl;
using DotCMIS.Exceptions;
using DotCMIS.Enums;

using Newtonsoft.Json;

using NUnit.Framework;

namespace TestLibrary.IntegrationTests
{
    /// <summary>
    /// Dot CMIS integration tests. Each method tests one specific test case. The test got to be finished after 15 mins, otherwise the test will fail.
    /// </summary>
    [TestFixture, Timeout(900000)]
    public class DotCMISTests
    {
        /// <summary>
        /// Disable HTTPS Verification
        /// </summary>
        [TestFixtureSetUp]
        public void ClassInit()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate {return true;};
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
        public void AppendContentStreamTest(string canonical_name, string localPath, string remoteFolderPath,
            string url, string user, string password, string repositoryId)
        {
            //var watch = Stopwatch.StartNew();
            ISession session = DotCMISSessionTests.CreateSession(user, password, url, repositoryId);
            //watch.Stop();
            //Console.WriteLine(String.Format("Created Session in {0} msec",watch.ElapsedMilliseconds));
            //watch.Restart();
            IFolder folder = (IFolder)session.GetObjectByPath(remoteFolderPath);
            //watch.Stop();
            //Console.WriteLine(String.Format("Requested folder in {0} msec", watch.ElapsedMilliseconds));
            string filename = "testfile.txt";
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, filename);
            properties.Add(PropertyIds.ObjectTypeId, "cmis:document");
            try{
                IDocument doc = session.GetObjectByPath(remoteFolderPath + "/" + filename) as IDocument;
                if (doc!=null) {
                    doc.Delete(true);
                    Console.WriteLine("Old file deleted");
                }
            }catch(Exception){}
            //watch.Restart();
            IDocument emptyDoc = folder.CreateDocument(properties, null, null);
            //watch.Stop();
            //Console.WriteLine(String.Format("Created empty doc in {0} msec", watch.ElapsedMilliseconds));
            Assert.That( emptyDoc.ContentStreamLength == 0 || emptyDoc.ContentStreamLength == null, "returned document shouldn't got any content");
            string content = "test";
            for(int i = 0; i < 10; i++) {
                ContentStream contentStream = new ContentStream();
                contentStream.FileName = filename;
                contentStream.MimeType = MimeType.GetMIMEType(filename);
                contentStream.Length = content.Length;
                using(var memstream = new MemoryStream(Encoding.UTF8.GetBytes(content))){
                    contentStream.Stream = memstream;
                    emptyDoc.AppendContentStream(contentStream, i==9, true);
                }
                Assert.AreEqual(content.Length * (i+1), emptyDoc.ContentStreamLength);
            }
            emptyDoc.DeleteAllVersions();
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
        public void GetRootFolderOfRepository(string canonical_name, string localPath, string remoteFolderPath,
            string url, string user, string password, string repositoryId)
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
        public void GetSyncPropertyFromFile(string canonical_name, string localPath, string remoteFolderPath,
            string url, string user, string password, string repositoryId)
        {
            ISession session = DotCMISSessionTests.CreateSession(user, password, url, repositoryId);
            IFolder folder = (IFolder)session.GetObjectByPath(remoteFolderPath);
            string filename = "testfile.txt";
            try{
                IDocument doc = session.GetObjectByPath(remoteFolderPath + "/" + filename) as IDocument;
                if (doc!=null) {
                    doc.Delete(true);
                    Console.WriteLine("Old file deleted");
                }
            }catch(CmisObjectNotFoundException){}
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, filename);
            properties.Add(PropertyIds.ObjectTypeId, "cmis:document");
            IList<string> devices = new List<string>();
            devices.Add("*");
            properties.Add("ignoreDeviceIds", devices);
            IList<string> ids = new List<string>();
            ids.Add("gds:sync");
            properties.Add(PropertyIds.SecondaryObjectTypeIds, ids);

            IDocument emptyDoc = folder.CreateDocument(properties, null, null);
            Assert.That(emptyDoc.ContentStreamLength == 0 || emptyDoc.ContentStreamLength == null);
            var context = new OperationContext();
            IDocument requestedDoc = session.GetObject(emptyDoc, context) as IDocument;
            bool propertyFound = false;
            foreach(var prop in requestedDoc.Properties)
            {
                if(prop.Id == "ignoreDeviceIds")
                {
                    propertyFound = true;
                    Assert.AreEqual("*", prop.FirstValue as string);
                }
            }
            Assert.IsTrue(propertyFound);
            emptyDoc.DeleteAllVersions();
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
        public void SetContentStreamTest(string canonical_name, string localPath, string remoteFolderPath,
            string url, string user, string password, string repositoryId)
        {
            //var watch = Stopwatch.StartNew();
            ISession session = DotCMISSessionTests.CreateSession(user, password, url, repositoryId);
            //watch.Stop();
            //Console.WriteLine(String.Format("Created Session in {0} msec",watch.ElapsedMilliseconds));
            //watch.Restart();
            IFolder folder = (IFolder)session.GetObjectByPath(remoteFolderPath);
            //watch.Stop();
            //Console.WriteLine(String.Format("Requested folder in {0} msec", watch.ElapsedMilliseconds));
            string filename = "testfile.txt";
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, filename);
            properties.Add(PropertyIds.ObjectTypeId, "cmis:document");
            try{
                IDocument doc = session.GetObjectByPath(remoteFolderPath + "/" + filename) as IDocument;
                if (doc!=null) {
                    doc.Delete(true);
                    Console.WriteLine("Old file deleted");
                }
            }catch(CmisObjectNotFoundException){}
            //watch.Restart();
            IDocument emptyDoc = folder.CreateDocument(properties, null, null);
            //watch.Stop();
            //Console.WriteLine(String.Format("Created empty doc in {0} msec", watch.ElapsedMilliseconds));
            Assert.That(emptyDoc.ContentStreamLength == null || emptyDoc.ContentStreamLength == 0, "returned document shouldn't got any content");
            string content = "";
            content += "Test ";
            ContentStream contentStream = new ContentStream();
            contentStream.FileName = filename;
            contentStream.MimeType = MimeType.GetMIMEType(filename);
            contentStream.Length = content.Length;
            using(var memstream = new MemoryStream(Encoding.UTF8.GetBytes(content))){
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
            using(var memstream = new MemoryStream(Encoding.UTF8.GetBytes(content))){
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
            using(var memstream = new MemoryStream(Encoding.UTF8.GetBytes(content))){
                contentStream.Stream = memstream;
                emptyDoc.SetContentStream(contentStream, true, true);
            }
            Assert.AreEqual(content.Length, emptyDoc.ContentStreamLength);
            emptyDoc.DeleteAllVersions();
        }

        [Test, TestCaseSource(typeof(ITUtils), "TestServers"), Category("Slow")]
        public void SetJPEGContentStream(string canonical_name, string localPath, string remoteFolderPath,
            string url, string user, string password, string repositoryId)
        {
            ISession session = DotCMISSessionTests.CreateSession(user, password, url, repositoryId);
            IFolder folder = (IFolder)session.GetObjectByPath(remoteFolderPath);
            string filename = "testfile.jpg";
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, filename);
            properties.Add(PropertyIds.ObjectTypeId, "cmis:document");
            try{
                IDocument doc = session.GetObjectByPath(remoteFolderPath + "/" + filename) as IDocument;
                if (doc!=null) {
                    doc.Delete(true);
                    Console.WriteLine("Old file deleted");
                }
            }catch(CmisObjectNotFoundException){}
            IDocument emptyDoc = folder.CreateDocument(properties, null, null);
            Assert.That(emptyDoc.ContentStreamLength == null || emptyDoc.ContentStreamLength == 0, "returned document shouldn't got any content");
            int contentLength = 1024;
            byte[] content = new byte[contentLength];
            using(RandomNumberGenerator random = RandomNumberGenerator.Create()){
                random.GetBytes(content);
            }
            ContentStream contentStream = new ContentStream();
            contentStream.FileName = filename;
            contentStream.MimeType = MimeType.GetMIMEType(filename);
            contentStream.Length = content.Length;
            using(var memstream = new MemoryStream(content)){
                contentStream.Stream = memstream;
                Console.WriteLine("content: " + memstream.Length);
                emptyDoc.SetContentStream(contentStream, true, true);
            }
            Assert.AreEqual(content.Length, emptyDoc.ContentStreamLength, "Setting content failed");
            IDocument randomDoc = session.GetObjectByPath(emptyDoc.Paths[0]) as IDocument;
            Assert.That (randomDoc != null);
            Assert.AreEqual (content.Length, randomDoc.ContentStreamLength, "Getting content on new object failed");
            emptyDoc.DeleteAllVersions();
        }
    }

    /// <summary>
    /// Dot CMIS session tests. Each log in process must be able to be executed in 60 seconds, otherwise the tests will fail.
    /// </summary>
    [TestFixture, Timeout(60000)]
    public class DotCMISSessionTests {
        /// <summary>
        /// Accept every SSL Server if an invalid cert is returned
        /// </summary>
        [TestFixtureSetUp]
        public void ClassInit()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate {return true;};
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
        public static ISession CreateSession(string user, Password password, string url, string repoId)
        {
            Dictionary<string, string> cmisParameters = new Dictionary<string, string>();
            cmisParameters[SessionParameter.BindingType] = BindingType.AtomPub;
            cmisParameters[SessionParameter.AtomPubUrl] = url;
            cmisParameters[SessionParameter.User] = user;
            cmisParameters[SessionParameter.Password] = password.ToString();
            cmisParameters[SessionParameter.RepositoryId] = repoId;
            // Sets the Connect Timeout to 60 secs
            cmisParameters[SessionParameter.ConnectTimeout] = "60000";

            ISession session =  SessionFactory.NewInstance().CreateSession(cmisParameters);
            HashSet<string> filters = new HashSet<string>();
            filters.Add("cmis:objectId");
            filters.Add("cmis:name");
            filters.Add("cmis:contentStreamFileName");
            filters.Add("cmis:contentStreamLength");
            filters.Add("cmis:lastModificationDate");
            filters.Add("cmis:path");
            filters.Add("cmis:changeToken");
            filters.Add(PropertyIds.SecondaryObjectTypeIds);
            HashSet<string> renditions = new HashSet<string>();
            renditions.Add("cmis:none");
            session.DefaultContext = session.CreateOperationContext(filters, false, true, false, IncludeRelationshipsFlag.None, null, true, null, true, 100);
            return session;
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
        public void CreateSession(string canonical_name, string localPath, string remoteFolderPath,
            string url, string user, string password, string repositoryId) {
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
        public void CreateSessionWithDeviceManagementAndUserAgent(string canonical_name, string localPath, string remoteFolderPath,
            string url, string user, string password, string repositoryId)
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
        public void CreateSessionWithCompressionEnabled(string canonical_name, string localPath, string remoteFolderPath,
            string url, string user, string password, string repositoryId)
        {
            Dictionary<string, string> cmisParameters = new Dictionary<string, string>();
            cmisParameters[SessionParameter.BindingType] = BindingType.AtomPub;
            cmisParameters[SessionParameter.AtomPubUrl] = url.ToString();
            cmisParameters[SessionParameter.User] = user;
            cmisParameters[SessionParameter.Password] = password;
            cmisParameters[SessionParameter.RepositoryId] = repositoryId;
            cmisParameters[SessionParameter.Compression] = Boolean.TrueString;
            // Sets the Connect Timeout to 60 secs
            cmisParameters[SessionParameter.ConnectTimeout] = "60000";
            // Sets the Read Timeout to 60 secs
            cmisParameters[SessionParameter.ReadTimeout] = "60000";
            SessionFactory.NewInstance().CreateSession(cmisParameters);
        }
    }
}

