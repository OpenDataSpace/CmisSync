using System;
using System.Net;

using CmisSync.Lib.Storage;

using DotCMIS.Binding;

using log4net;

namespace CmisSync.Lib.Cmis
{

    // TODO Refactore this class because it is a simple copy of PersistentStandardAuthenticationProvider
    // => Extract methods and call them instead of the duplicated code

    /// <summary>
    /// Persistent ntlm authentication provider.
    /// </summary>
    public class PersistentNtlmAuthenticationProvider : NtlmAuthenticationProvider, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PersistentStandardAuthenticationProvider));

        private ICookieStorage Storage;
        private bool disposed = false;
        private Uri Url;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Cmis.PersistentNtlmAuthenticationProvider"/> class.
        /// </summary>
        /// <param name='storage'>
        /// Storage to save the cookies to.
        /// </param>
        /// <param name='url'>
        /// URL of the cookies.
        /// </param>
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

        /// <summary>
        /// Handles the response, if it is a <see cref="System.Net.HttpWebResponse"/> instance.
        /// Takes all cookies of the response and saves them at the local <see cref="System.Net.CookieContainer"/>.
        /// </summary>
        /// <param name='connection'>
        /// Connection.
        /// </param>
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
        /// Releases all resource used by the <see cref="CmisSync.Lib.Cmis.PersistentNtlmAuthenticationProvider"/> object.
        /// </summary>
        /// <remarks>
        /// Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="CmisSync.Lib.Cmis.PersistentNtlmAuthenticationProvider"/>. The <see cref="Dispose"/> method
        /// leaves the <see cref="CmisSync.Lib.Cmis.PersistentNtlmAuthenticationProvider"/> in an unusable state. After
        /// calling <see cref="Dispose"/>, you must release all references to the
        /// <see cref="CmisSync.Lib.Cmis.PersistentNtlmAuthenticationProvider"/> so the garbage collector can reclaim
        /// the memory that the <see cref="CmisSync.Lib.Cmis.PersistentNtlmAuthenticationProvider"/> was occupying.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the specified disposing.
        /// </summary>
        /// <param name='disposing'>
        /// Disposing.
        /// </param>
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

