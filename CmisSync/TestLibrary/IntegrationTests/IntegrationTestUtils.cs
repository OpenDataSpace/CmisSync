//-----------------------------------------------------------------------
// <copyright file="IntegrationTestUtils.cs" company="GRAU DATA AG">
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
                var path = GetServerPath();

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

        public static dynamic GetConfig()
        {
            var path = GetServerPath();
            return JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(path))[0];
        }

        private static string GetServerPath()
        {
            string path = "../../test-servers.json";
            bool exists = File.Exists(path);

            if (!exists)
            {
                path = "../CmisSync/TestLibrary/test-servers.json";
            }

            return path;
        }
    }
}
