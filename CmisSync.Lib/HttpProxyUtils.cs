using System;
using System.Net;

using log4net;

namespace CmisSync.Lib
{
    public class HttpProxyUtils
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HttpProxyUtils));

        public static void SetDefaultProxy(Config.ProxySettings settings)
        {
            try{
            if(settings.Selection == Config.ProxySelection.SYSTEM)
            {
                WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy();
            }
            else if(settings.Selection == Config.ProxySelection.NOPROXY)
            {
                WebRequest.DefaultWebProxy = null;
            }
            else
            {
                WebProxy proxy = new WebProxy();
                proxy.Address = settings.Server;
                if(settings.LoginRequired)
                {
                    var password = new Credentials.Password() {ObfuscatedPassword = settings.ObfuscatedPassword};
                    proxy.Credentials = new NetworkCredential(settings.Username, password.ToString());
                }
                WebRequest.DefaultWebProxy = proxy;
                }
            }catch (Exception e) {
                Logger.Warn("Failed to set the default proxy, please check your proxy config: ", e);
            }
        }
    }
}

