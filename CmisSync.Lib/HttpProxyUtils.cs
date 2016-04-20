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
        private static IWebProxy systemDefault;
        private static bool isSystemDefaultSet = false;
        private static object l = new object();

        /// <summary>
        /// Inits the proxy switching support. Must be called before any default proxy manipulation is done.
        /// If this class is the only class, which touches the proxy settings, it is not needed to be called.
        /// </summary>
        public static void InitProxySwitchingSupport() {
            if (!isSystemDefaultSet) {
                lock(l) {
                    if (!isSystemDefaultSet) {
                        systemDefault = WebRequest.DefaultWebProxy;
                        isSystemDefaultSet = true;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the default proxy for every HTTP request.
        /// If the caller would like to know if the call throws any exception, the second parameter should be set to true
        /// </summary>
        /// <param name="settings">proxy settings.</param>
        /// <param name="throwExceptions">If set to <c>true</c> throw exceptions.</param>
        public static void SetDefaultProxy(Config.ProxySettings settings, bool throwExceptions = false) {
            InitProxySwitchingSupport();
            try {
                switch (settings.Selection) {
                case ProxySelection.CUSTOM:
                    IWebProxy proxy = new WebProxy(settings.Server);
                    if (settings.LoginRequired) {
                        proxy.Credentials = new NetworkCredential(settings.Username, Crypto.Deobfuscate(settings.ObfuscatedPassword));
                    }

                    WebRequest.DefaultWebProxy = proxy;
                    return;
                case ProxySelection.NOPROXY:
                    WebRequest.DefaultWebProxy = null;
                    return;
                case ProxySelection.SYSTEM:
                    WebRequest.DefaultWebProxy = systemDefault;
                    return;
                }
            } catch (Exception e) {
                if (throwExceptions) {
                    throw;
                }

                Logger.Warn("Failed to set the default proxy, please check your proxy config: ", e);
            }
        }
    }
}