using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.IO;
using System.Text;


using CmisSync.Lib;
using CmisSync.Lib.Cmis;
using CmisSync.Lib.Sync;

using DotCMIS;
using DotCMIS.Client;
using DotCMIS.Client.Impl;
using DotCMIS.Data;
using DotCMIS.Data.Impl;


using NUnit.Framework;

namespace TestLibrary.IntegrationTests
{
    [TestFixture]
    public class DotCMISTests
    {
        private readonly string CMISSYNCDIR = ConfigManager.CurrentConfig.FoldersPath;

        class TrustAlways : ICertificatePolicy
        {
            public bool CheckValidationResult(ServicePoint sp, X509Certificate certificate, WebRequest request, int error)
            {
                // For testing, always accept any certificate
                return true;
            }
        }

        private ISession CreateSession(RepoInfo repoInfo)
        {
            Dictionary<string, string> cmisParameters = new Dictionary<string, string>();
            cmisParameters[SessionParameter.BindingType] = BindingType.AtomPub;
            cmisParameters[SessionParameter.AtomPubUrl] = repoInfo.Address.ToString();
            cmisParameters[SessionParameter.User] = repoInfo.User;
            cmisParameters[SessionParameter.Password] = repoInfo.Password.ToString();
            cmisParameters[SessionParameter.RepositoryId] = repoInfo.RepoID;
            cmisParameters[SessionParameter.ConnectTimeout] = "-1";

            return SessionFactory.NewInstance().CreateSession(cmisParameters);
        }


        [TestFixtureSetUp]
        public void ClassInit()
        {
            ServicePointManager.CertificatePolicy = new TrustAlways();
        }

        [Test, TestCaseSource("TestServers"), Category("Slow")]
        public void AppendContentStreamTest(string canonical_name, string localPath, string remoteFolderPath,
            string url, string user, string password, string repositoryId)
        {
            RepoInfo repoInfo = new RepoInfo(
                canonical_name,
                CMISSYNCDIR,
                remoteFolderPath,
                url,
                user,
                password,
                repositoryId,
                5000);
            ISession session = CreateSession(repoInfo);
            IFolder folder = (IFolder)session.GetObjectByPath(remoteFolderPath);
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
            IDocument emptyDoc = folder.CreateDocument(properties, null, null);
            Console.WriteLine("Empty file created");
            Assert.AreEqual(0, emptyDoc.ContentStreamLength);
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
    }
}

