using System.Net;

namespace TestLibrary.UtilsTests
{
    using System;
    using System.Security.Cryptography.X509Certificates;

    using NUnit.Framework;

    [TestFixture, Category("Medium")]
    public class X509StoreTest
    {
        private X509Store underTest;
        private X509Certificate2 oldCert;
        private X509Certificate2 newCert;
        private readonly string url = "https://demo.deutsche-wolke.de";

        [TestFixtureSetUp]
        public void SetUpCertificates() {
            this.oldCert = new X509Certificate2(Convert.FromBase64String("MIIFPzCCBCegAwIBAgIDCE5zMA0GCSqGSIb3DQEBBQUAMDwxCzAJBgNVBAYTAlVTMRcwFQYDVQQK\nEw5HZW9UcnVzdCwgSW5jLjEUMBIGA1UEAxMLUmFwaWRTU0wgQ0EwHhcNMTIwOTE3MTYzMzQ3WhcN\nMTQwOTIxMDMyNzU4WjCBwjEpMCcGA1UEBRMgczE0M2VYb1kzdzBnaExTdGdoYUt2WC1jdUwzcEdn\nQXQxEzARBgNVBAsTCkdUNTA0MDMwNzIxMTAvBgNVBAsTKFNlZSB3d3cucmFwaWRzc2wuY29tL3Jl\nc291cmNlcy9jcHMgKGMpMTIxLzAtBgNVBAsTJkRvbWFpbiBDb250cm9sIFZhbGlkYXRlZCAtIFJh\ncGlkU1NMKFIpMRwwGgYDVQQDDBMqLmRldXRzY2hlLXdvbGtlLmRlMIIBIjANBgkqhkiG9w0BAQEF\nAAOCAQ8AMIIBCgKCAQEA5Y3ON7vF/6edAgBQ0mznEL/6QudyDUJGUfI5eZ15igjK0LurhzFxUQQB\nzMIFVsACWuwGfZu87LZpIAxmVmS3gArToKncPZ9ONYFiEtvsKXrp+QUhd01kYcddkRgk6bvYiBq5\nlE5Q3O40JoIHHOXq8VycMH+PBgxmPyGaoRVwkxOjUtMZttZ3IRXaTl51DLxQirOpvSJiHTytQlbd\nuZ3xEepBSApIkZ4cECI39LWVQrvvNqRdCFvEQHiTadh7m02xH5ue32GZvvgsTf823imT9ThLSjM4\ne0O2vVDzTVbn/j3Xxbe8OI9EcKWkh0jUVGGGIQ7iCm72b4G5HpKGruyh7wIDAQABo4IBwTCCAb0w\nHwYDVR0jBBgwFoAUa2k9ahhCSt2PAmU5/TUkhniRFjAwDgYDVR0PAQH/BAQDAgWgMB0GA1UdJQQW\nMBQGCCsGAQUFBwMBBggrBgEFBQcDAjAxBgNVHREEKjAoghMqLmRldXRzY2hlLXdvbGtlLmRlghFk\nZXV0c2NoZS13b2xrZS5kZTBDBgNVHR8EPDA6MDigNqA0hjJodHRwOi8vcmFwaWRzc2wtY3JsLmdl\nb3RydXN0LmNvbS9jcmxzL3JhcGlkc3NsLmNybDAdBgNVHQ4EFgQUYWxxKbNBronjdpKFcuPxrHq+\n30MwDAYDVR0TAQH/BAIwADB4BggrBgEFBQcBAQRsMGowLQYIKwYBBQUHMAGGIWh0dHA6Ly9yYXBp\nZHNzbC1vY3NwLmdlb3RydXN0LmNvbTA5BggrBgEFBQcwAoYtaHR0cDovL3JhcGlkc3NsLWFpYS5n\nZW90cnVzdC5jb20vcmFwaWRzc2wuY3J0MEwGA1UdIARFMEMwQQYKYIZIAYb4RQEHNjAzMDEGCCsG\nAQUFBwIBFiVodHRwOi8vd3d3Lmdlb3RydXN0LmNvbS9yZXNvdXJjZXMvY3BzMA0GCSqGSIb3DQEB\nBQUAA4IBAQBnQEt0eAq0+JvFQYyWD9PeAS0Rh4vnsrDERfKsy4lsQ8mJL1vhx9z43KYavyqmAqfE\nKosZ+YbH7gXoW2h7NL2PTKwcjPfZ+iXg5Yr8XXBLpK0kL2/j7uINyFmy7qnyLleMTsTjAjhLrP8c\n+tT6GM27xBP9/fuA5Js3zYBg7bqwMQxxG6Z6hlOuYoI3uZcMDX3cgQGaBsAjqTTS1mBax25KVGrN\naYTa6Rtzth80/RJ/bGtk2+Jo55iqIxBy5FVI9o5HCCQJpxddg3LZ/OU8zK1SAvri+dN8sRStF4+J\ncnITle3v8Ob5F1vOBrds1kDOpHMIfveW+to4mN7cLZqcImud"));
            this.newCert = new X509Certificate2(Convert.FromBase64String("MIIFPzCCBCegAwIBAgIDFWFgMA0GCSqGSIb3DQEBBQUAMDwxCzAJBgNVBAYTAlVTMRcwFQYDVQQK\nEw5HZW9UcnVzdCwgSW5jLjEUMBIGA1UEAxMLUmFwaWRTU0wgQ0EwHhcNMTQwOTIxMjA0ODI5WhcN\nMTUxMDI0MTA1NDExWjCBwjEpMCcGA1UEBRMgTEFhQ0xFN1lsWGhaby1Udjlka0pJQnlxMDVZei02\nOU0xEzARBgNVBAsTCkdUNTA0MDMwNzIxMTAvBgNVBAsTKFNlZSB3d3cucmFwaWRzc2wuY29tL3Jl\nc291cmNlcy9jcHMgKGMpMTQxLzAtBgNVBAsTJkRvbWFpbiBDb250cm9sIFZhbGlkYXRlZCAtIFJh\ncGlkU1NMKFIpMRwwGgYDVQQDDBMqLmRldXRzY2hlLXdvbGtlLmRlMIIBIjANBgkqhkiG9w0BAQEF\nAAOCAQ8AMIIBCgKCAQEA5Y3ON7vF/6edAgBQ0mznEL/6QudyDUJGUfI5eZ15igjK0LurhzFxUQQB\nzMIFVsACWuwGfZu87LZpIAxmVmS3gArToKncPZ9ONYFiEtvsKXrp+QUhd01kYcddkRgk6bvYiBq5\nlE5Q3O40JoIHHOXq8VycMH+PBgxmPyGaoRVwkxOjUtMZttZ3IRXaTl51DLxQirOpvSJiHTytQlbd\nuZ3xEepBSApIkZ4cECI39LWVQrvvNqRdCFvEQHiTadh7m02xH5ue32GZvvgsTf823imT9ThLSjM4\ne0O2vVDzTVbn/j3Xxbe8OI9EcKWkh0jUVGGGIQ7iCm72b4G5HpKGruyh7wIDAQABo4IBwTCCAb0w\nHwYDVR0jBBgwFoAUa2k9ahhCSt2PAmU5/TUkhniRFjAwDgYDVR0PAQH/BAQDAgWgMB0GA1UdJQQW\nMBQGCCsGAQUFBwMBBggrBgEFBQcDAjAxBgNVHREEKjAoghMqLmRldXRzY2hlLXdvbGtlLmRlghFk\nZXV0c2NoZS13b2xrZS5kZTBDBgNVHR8EPDA6MDigNqA0hjJodHRwOi8vcmFwaWRzc2wtY3JsLmdl\nb3RydXN0LmNvbS9jcmxzL3JhcGlkc3NsLmNybDAdBgNVHQ4EFgQUYWxxKbNBronjdpKFcuPxrHq+\n30MwDAYDVR0TAQH/BAIwADB4BggrBgEFBQcBAQRsMGowLQYIKwYBBQUHMAGGIWh0dHA6Ly9yYXBp\nZHNzbC1vY3NwLmdlb3RydXN0LmNvbTA5BggrBgEFBQcwAoYtaHR0cDovL3JhcGlkc3NsLWFpYS5n\nZW90cnVzdC5jb20vcmFwaWRzc2wuY3J0MEwGA1UdIARFMEMwQQYKYIZIAYb4RQEHNjAzMDEGCCsG\nAQUFBwIBFiVodHRwOi8vd3d3Lmdlb3RydXN0LmNvbS9yZXNvdXJjZXMvY3BzMA0GCSqGSIb3DQEB\nBQUAA4IBAQAFZbp+uGdfjEBlqLMxd2VzOJTpt38V6ixZIC39zNuP30PoAq8XBR96T7TkRsMvSE7J\n2ITiG1XMCVA9dZR3Jzmm+lXeyVubIVDhJXo+ko/ndPkAlkh/tzLjckxU5nyRKvsW8YdBgptxpoLp\nYbFlZLL2uWy31GE/6qxGxzSp6FXaLdKBbWqZAitfNo+MH/33dRRsG4dkhAS7XWZUSKDIGGcPeeGD\nabd9zjclNvfwl9lcm52cQagiRiP6EIwXMhqhQDWGDnGjL+7SCT1u7BFFq76DDnDNwWShd8OVLsEM\nPx+Ckfv6Fj5qcs06iEJFCwomXB77aWmKlYdVhMTVGDiE52uk"));
        }

        [SetUp]
        public void SetUp() {
            this.underTest = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            this.underTest.Open(OpenFlags.ReadWrite);
        }

        [TearDown]
        public void CloseStore() {
            this.underTest.Remove(this.oldCert);
            this.underTest.Remove(this.newCert);
            Assert.That(this.underTest.Certificates.Contains(this.oldCert), Is.False);
            Assert.That(this.underTest.Certificates.Contains(this.newCert), Is.False);
            this.underTest.Close();
        }

        [Test]
        public void AddingCertsToStore()
        {
            this.underTest.Add(this.oldCert);
            Assert.That(this.underTest.Certificates.Contains(this.oldCert));
            Assert.That(this.underTest.Certificates.Contains(this.newCert), Is.False);
            this.underTest.Add(this.newCert);
            Assert.That(this.underTest.Certificates.Contains(this.oldCert));
            Assert.That(this.underTest.Certificates.Contains(this.newCert));
        }

        [Test, Category("Slow"), Ignore("Should fail but doesn't")]
        public void HttpsWebRequest() {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using(response.GetResponseStream());
        }
    }
}