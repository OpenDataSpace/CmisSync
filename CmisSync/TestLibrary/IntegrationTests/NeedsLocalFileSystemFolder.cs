
namespace TestLibrary.IntegrationTests
{
    using System;
    using System.IO;

    using TestLibrary.TestUtils;

    public abstract class NeedsLocalFileSystemFolder : IsTestWithConfiguredLog4Net
    {
        private static dynamic config;

        protected DirectoryInfo LocalTestDir { get; private set; }

        protected void TestFixtureSetUp() {
            config = ITUtils.GetConfig();
        }

        protected DirectoryInfo InitLocalTestDir() {
            if (config == null) {
                this.TestFixtureSetUp();
            }

            string subfolder = this.ToString() + "_" + Guid.NewGuid().ToString();
            this.LocalTestDir = new DirectoryInfo(Path.Combine(config[1].ToString(), subfolder));
            this.LocalTestDir.Create();
            return this.LocalTestDir;
        }

        protected void RemoveLocalTestDir() {
            if (this.LocalTestDir.Exists) {
                this.LocalTestDir.Delete(true);
            }
        }
    }
}