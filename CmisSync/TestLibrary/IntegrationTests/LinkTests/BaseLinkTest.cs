using DotCMIS.Client;


namespace TestLibrary.IntegrationTests.LinkTests {
    using System;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, Category("Links"), Category("Slow")]
    public abstract class BaseLinkTest : BaseFullRepoTest {
        [SetUp]
        public void EnsureThatDownloadLinksAreSupported() {
            if (!this.session.AreLinksSupported()) {
                Assert.Ignore("Server does not support to create download link");
            }
        }
    }
}