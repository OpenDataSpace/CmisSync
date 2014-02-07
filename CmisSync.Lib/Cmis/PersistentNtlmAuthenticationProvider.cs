using System;
using System.Net;

using CmisSync.Lib.Storage;

using DotCMIS.Binding;

using log4net;

namespace CmisSync.Lib.Cmis
{

    // TODO Refactore this class because it is a simple copy of PersistentStandardAuthenticationProvider
    // => Extract methods and call them instead of the duplicated code
    public class PersistentNtlmAuthenticationProvider : NtlmAuthenticationProvider, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PersistentStandardAuthenticationProvider));

        private ICookieStorage Storage;
        private bool disposed = false;
        private Uri Url;

        public PersistentNtlmAuthenticationProvider (ICookieStorage storage, Uri url)
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

