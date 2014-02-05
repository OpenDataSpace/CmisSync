using System;
using System.Net;

namespace CmisSync.Lib.Storage
{
    public interface ICookieStorage
    {
        CookieCollection Cookies { get; set;}
    }

    [Obsolete("Please use this class only until the persistent one is implemented")]
    public class TemporaryCookieStorage : ICookieStorage
    {
        public virtual CookieCollection Cookies { get; set;}
    }
}

