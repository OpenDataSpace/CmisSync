using System;
using System.Net;
using log4net;

namespace CmisSync.Lib
{
    /// <summary>
    /// Http proxy utils.
    /// </summary>
    public class HttpProxyUtils
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HttpProxyUtils));

        /// <summary>
        /// Sets the default proxy for every HTTP request.
        /// If the caller would like to know if the call throws any exception, the second parameter should be set to true
        /// </summary>
        /// <param name="settings">Settings.</param>
        /// <param name="throwExceptions">If set to <c>true</c> throw exceptions.</param>
        public static void SetDefaultProxy(Config.ProxySettings settings, bool throwExceptions = false)
        {
            try
            {
                IWebProxy proxy = null;
                switch(settings.Selection) {
                    case Config.ProxySelection.SYSTEM:
                        proxy = WebRequest.GetSystemWebProxy();
                        break;
                    case Config.ProxySelection.CUSTOM:
                        proxy = new WebProxy();
                        (proxy as WebProxy).Address = settings.Server;
                        break;
                }
                if (settings.LoginRequired && proxy != null)
                    proxy.Credentials = new NetworkCredential(settings.Username, Crypto.Deobfuscate(settings.ObfuscatedPassword));
                WebRequest.DefaultWebProxy = proxy;
            }
            catch (Exception e)
            {
                if (throwExceptions)
                    throw;
                Logger.Warn("Failed to set the default proxy, please check your proxy config: ", e);
            }
        }
    }
}

