
namespace TestLibrary.IntegrationTests.PrivateWorkingCopyTests {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;

    using NUnit.Framework;

    using TestUtils;
    [TestFixture, Timeout(180000), TestName("PWC")]
    public class UploadChangedContent : BaseFullRepoTest {
        [Test, Category("Slow"), Ignore("Not yet implemented")]
        public void UploadFileChangeContentAbortUpdateContinueUpload() {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();

            this.InitializeAndRunRepo(swallowExceptions: true);
        }
    }
}