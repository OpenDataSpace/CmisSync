using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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

using Newtonsoft.Json;

using NUnit.Framework;



namespace TestLibrary.IntegrationTests
{
    [TestFixture]
    public class DotCMISTests
    {

        [TestFixtureSetUp]
        public void ClassInit()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate {return true;};
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            ServicePointManager.ServerCertificateValidationCallback = null;
        }


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
            Assert.AreEqual(0, emptyDoc.ContentStreamLength, "returned document shouldn't got any content");
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

        [Ignore]
        [Test, TestCaseSource(typeof(ITUtils), "TestServers"), Category("Slow")]
        public void GetSyncPropertyFromFile(string canonical_name, string localPath, string remoteFolderPath,
            string url, string user, string password, string repositoryId)
        {
            ISession session = DotCMISSessionTests.CreateSession(user, password, url, repositoryId);
            IFolder folder = (IFolder)session.GetObjectByPath(remoteFolderPath);
            string filename = "testfile.txt";
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, filename);
            properties.Add(PropertyIds.BaseTypeId, "cmis:document");
            properties.Add(PropertyIds.ObjectTypeId, "gds:sync");
            properties.Add("cmis:secondaryObjectTypeIds", "*");
            try{
                IDocument doc = session.GetObjectByPath(remoteFolderPath + "/" + filename) as IDocument;
                if (doc!=null) {
                    doc.Delete(true);
                    Console.WriteLine("Old file deleted");
                }
            }catch(CmisObjectNotFoundException){}
            IDocument emptyDoc = folder.CreateDocument(properties, null, null);
            Console.WriteLine("Empty file created");
            Assert.AreEqual(0, emptyDoc.ContentStreamLength);
            var context = new OperationContext();
            session.GetObject(emptyDoc, context);
            emptyDoc.DeleteAllVersions();
        }

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
            Assert.AreEqual(0, emptyDoc.ContentStreamLength, "returned document shouldn't got any content");
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
    }

    /// <summary>
    /// Dot CMIS session tests. Each log in process must be able to be executed in 10 seconds, otherwise the tests will fail
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
            cmisParameters[SessionParameter.ConnectTimeout] = "-1";

            return SessionFactory.NewInstance().CreateSession(cmisParameters);
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
            // Sets the Connect Timeout to infinite
            cmisParameters[SessionParameter.ConnectTimeout] = "10000";
            // Sets the Read Timeout to infinite
            cmisParameters[SessionParameter.ReadTimeout] = "10000";
            cmisParameters[SessionParameter.DeviceIdentifier] = ConfigManager.CurrentConfig.DeviceId.ToString();
            cmisParameters[SessionParameter.UserAgent] = Utils.CreateUserAgent();
            SessionFactory.NewInstance().CreateSession(cmisParameters);
        }

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
            // Sets the Connect Timeout to infinite
            cmisParameters[SessionParameter.ConnectTimeout] = "10000";
            // Sets the Read Timeout to infinite
            cmisParameters[SessionParameter.ReadTimeout] = "10000";
            SessionFactory.NewInstance().CreateSession(cmisParameters);
        }
    }
}

