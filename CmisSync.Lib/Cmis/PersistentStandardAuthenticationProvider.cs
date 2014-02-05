using System;
using System.Net;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

using DotCMIS.Binding;

using log4net;

using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Cmis
{
    public class PersistentStandardAuthenticationProvider : StandardAuthenticationProvider, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PersistentStandardAuthenticationProvider));

        private ICookieStorage Storage;
        private bool disposed = false;
        private Uri Url;

        public PersistentStandardAuthenticationProvider (ICookieStorage storage, Uri url)
        {
            if(storage == null)
                throw new ArgumentNullException("Given db is null");
            if(url == null)
                throw new ArgumentNullException("Given URL is null");
            Storage = storage;
            Url = url;
            foreach(Cookie c in Storage.Cookies)
                this.Cookies.Add(c);
        }

        public override void HandleResponse(object connection)
        {
            HttpWebResponse response = connection as HttpWebResponse;
            if (response != null)
            {
                // AtomPub and browser binding authentictaion
                this.Cookies.Add(response.Cookies);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!this.disposed)
            {
                if(disposing)
                {
                    // Dispose managed resources.
                    try{
                        Storage.Cookies = Cookies.GetCookies(Url);
                    }catch(Exception e) {
                        Logger.Debug(String.Format("Failed to save session cookies of \"{0}\" in db", Url.AbsolutePath), e);
                    }
                }
                disposed = true;
            }
        }
    }
}

