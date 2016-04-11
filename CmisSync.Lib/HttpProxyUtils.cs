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
        private static bool appRestartNeeded = false;

        /// <summary>
        /// Sets the default proxy for every HTTP request.
        /// If the caller would like to know if the call throws any exception, the second parameter should be set to true
        /// </summary>
        /// <param name="settings">proxy settings.</param>
        /// <param name="throwExceptions">If set to <c>true</c> throw exceptions.</param>
        public static void SetDefaultProxy(Config.ProxySettings settings, bool throwExceptions = false) {
            try {
                if (appRestartNeeded && settings.Selection == ProxySelection.SYSTEM) {
                    throw new ProtocolViolationException("Restart App to be able to use the new proxy settings");
                }
                IWebProxy proxy = null;
                switch (settings.Selection) {
                    case Config.ProxySelection.CUSTOM:
                        proxy = new WebProxy(settings.Server);
                        appRestartNeeded = true;
                        break;
                    case ProxySelection.NOPROXY:
                        appRestartNeeded = true;
                        break;
                    default:
                        return;
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