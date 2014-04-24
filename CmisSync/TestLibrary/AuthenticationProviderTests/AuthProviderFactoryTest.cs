
namespace TestLibrary.AuthenticationProviderTests
{
    using System;

    using CmisSync.Lib;
    using CmisSync.Lib.Cmis;

    using DBreeze;

    using Newtonsoft.Json;

    using NUnit.Framework;

    [TestFixture]
    public class AuthProviderFactoryTest
    {
        private DBreezeEngine engine;
        private readonly Uri url = new Uri("https://example.com");

        [TestFixtureSetUp]
        public void InitCustomSerializator()
        {
            // Use Newtonsoft.Json as Serializator
            DBreeze.Utils.CustomSerializator.Serializator = JsonConvert.SerializeObject; 
            DBreeze.Utils.CustomSerializator.Deserializator = JsonConvert.DeserializeObject;
        }

        [SetUp]
        public void SetUp()
        {
            this.engine = new DBreezeEngine(new DBreezeConfiguration{ Storage = DBreezeConfiguration.eStorage.MEMORY });
        }

        [TearDown]
        public void TearDown()
        {
            this.engine.Dispose();
        }

        [Test, Category("Fast")]
        public void CreateBasicAuthProvider()
        {
            var provider = AuthProviderFactory.CreateAuthProvider(Config.AuthenticationType.BASIC, this.url, this.engine);
            Assert.That(provider, Is.TypeOf<PersistentStandardAuthenticationProvider>());
        }

        [Test, Category("Fast")]
        public void CreateNtlmAuthProvider()
        {
            var provider = AuthProviderFactory.CreateAuthProvider(Config.AuthenticationType.NTLM, this.url, this.engine);
            Assert.That(provider, Is.TypeOf<PersistentNtlmAuthenticationProvider>());
        }

        [Test, Category("Fast")]
        public void CreateKerberosAuthProvider()
        {
            var provider = AuthProviderFactory.CreateAuthProvider(Config.AuthenticationType.KERBEROS, this.url, this.engine);
            Assert.That(provider, Is.TypeOf<PersistentNtlmAuthenticationProvider>());
        }

        [Test, Category("Fast")]
        public void CreateUnimplementedAuthTypeReturnDefaultAuthProvider()
        {
            var provider = AuthProviderFactory.CreateAuthProvider(Config.AuthenticationType.SHIBBOLETH, this.url, this.engine);
            Assert.That(provider, Is.TypeOf<StandardAuthenticationProviderWrapper>());
        }
    }
}
