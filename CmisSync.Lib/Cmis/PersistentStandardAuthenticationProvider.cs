using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;

using DotCMIS.Binding;

using log4net;

using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Cmis
{
    /// <summary>
    /// Persistent standard authentication provider.
    /// </summary>
    public class PersistentStandardAuthenticationProvider : StandardAuthenticationProvider, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PersistentStandardAuthenticationProvider));

        private ICookieStorage Storage;
        private bool disposed = false;
        private Uri Url;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Cmis.PersistentStandardAuthenticationProvider"/> class.
        /// </summary>
        /// <param name="storage">Storage.</param>
        /// <param name="url">URL.</param>
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
        /// <summary>
        /// Handles the HttpWebResponse by extracting the cookies.
        /// </summary>
        /// <param name="connection">Connection.</param>
        public override void HandleResponse(object connection)
        {
            HttpWebResponse response = connection as HttpWebResponse;
            if (response != null)
            {
                // AtomPub and browser binding authentictaion
                this.Cookies.Add(response.Cookies);
            }
        }
        /// <summary>
        /// Releases all resource used by the <see cref="CmisSync.Lib.Cmis.PersistentStandardAuthenticationProvider"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="CmisSync.Lib.Cmis.PersistentStandardAuthenticationProvider"/>. The <see cref="Dispose"/> method
        /// leaves the <see cref="CmisSync.Lib.Cmis.PersistentStandardAuthenticationProvider"/> in an unusable state.
        /// After calling <see cref="Dispose"/>, you must release all references to the
        /// <see cref="CmisSync.Lib.Cmis.PersistentStandardAuthenticationProvider"/> so the garbage collector can
        /// reclaim the memory that the <see cref="CmisSync.Lib.Cmis.PersistentStandardAuthenticationProvider"/> was occupying.</remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose the specified disposing.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>
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

