
namespace TestLibrary.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Newtonsoft.Json;

    /// <summary>
    /// Helper functions for integration tests
    /// </summary>
    public class ITUtils
    {
        /// <summary>
        /// Gets the test servers configuration with repository id in json file "test-servers.json".
        /// </summary>
        /// <value>
        /// The test servers.
        /// </value>
        public static IEnumerable<object[]> TestServers
        {
            get
            {
                string path = "../../test-servers.json";
                bool exists = File.Exists(path);

                if (!exists)
                {
                    path = "../CmisSync/TestLibrary/test-servers.json";
                }

                return JsonConvert.DeserializeObject<List<object[]>>(
                    File.ReadAllText(path));
            }
        }

        /// <summary>
        /// Gets the test server credentials for the fuzzy server test. Reads it information form the
        /// "test-servers-fuzzy.json" file.
        /// </summary>
        /// <value>
        /// The test servers fuzzy.
        /// </value>
        public static IEnumerable<object[]> TestServersFuzzy
        {
            get
            {
                string path = "../../test-servers-fuzzy.json";
                bool exists = File.Exists(path);

                if (!exists)
                {
                    path = "../CmisSync/TestLibrary/test-servers-fuzzy.json";
                }

                return JsonConvert.DeserializeObject<List<object[]>>(
                    File.ReadAllText(path));
            }
        }

        /// <summary>
        /// Gets the proxy server settings saved in "proxy-server.json" file.
        /// </summary>
        /// <value>The proxy server.</value>
        public static IEnumerable<object[]> ProxyServer
        {
            get
            {
                string path = "../../proxy-server.json";
                bool exists = File.Exists(path);

                if (!exists)
                {
                    path = "../CmisSync/TestLibrary/proxy-server.json";
                }

                return JsonConvert.DeserializeObject<List<object[]>>(
                    File.ReadAllText(path));
            }
        }
    }
}