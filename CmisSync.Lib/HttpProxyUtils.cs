//-----------------------------------------------------------------------
// <copyright file="HttpProxyUtils.cs" company="GRAU DATA AG">
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
namespace CmisSync.Lib {
    using System;
    using System.Net;
    
    using log4net;
    using CmisSync.Lib.Config;

    /// <summary>
    /// Http proxy utils.
    /// </summary>
    public static class HttpProxyUtils {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HttpProxyUtils));

        /// <summary>
        /// Sets the default proxy for every HTTP request.
        /// If the caller would like to know if the call throws any exception, the second parameter should be set to true
        /// </summary>
        /// <param name="settings">proxy settings.</param>
        /// <param name="throwExceptions">If set to <c>true</c> throw exceptions.</param>
        public static void SetDefaultProxy(Config.ProxySettings settings, bool throwExceptions = false) {
            try {
                IWebProxy proxy = null;
                switch (settings.Selection) {
                    case Config.ProxySelection.SYSTEM:
                        proxy = WebRequest.GetSystemWebProxy();
                        break;
                    case Config.ProxySelection.CUSTOM:
                        proxy = new WebProxy(settings.Server);
                        break;
                }

                if (settings.LoginRequired && proxy != null) {
                    proxy.Credentials = new NetworkCredential(settings.Username, Crypto.Deobfuscate(settings.ObfuscatedPassword));
                }

                WebRequest.DefaultWebProxy = proxy;
            } catch (Exception e) {
                if (throwExceptions) {
                    throw;
                }

                Logger.Warn("Failed to set the default proxy, please check your proxy config: ", e);
            }
        }
    }
}