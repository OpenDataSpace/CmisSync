using System;
using DotCMIS.Binding;
using CmisSync.Lib.Cmis;
using System.Net;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

using log4net;

namespace CmisSync.Lib
{
    public class PersistentStandardAuthenticationProvider : StandardAuthenticationProvider, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PersistentStandardAuthenticationProvider));

        private IDatabase Db;
        private bool disposed = false;
        private Uri Url;

        public PersistentStandardAuthenticationProvider (IDatabase db, Uri url)
        {
            if(db == null)
                throw new ArgumentNullException("Given db is null");
            if(url == null)
                throw new ArgumentNullException("Given URL is null");
            Db = db;
            Url = url;
            var cookies = Db.GetSessionCookies();
            foreach(Cookie c in cookies)
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
                        Db.SetSessionCookies(Cookies.GetCookies(Url));
                    }catch(Exception e) {
                        Logger.Debug(String.Format("Failed to save session cookies of \"{0}\" in db", Url.AbsolutePath), e);
                    }
                }
                disposed = true;
            }
        }
    }
}

