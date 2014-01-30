using System;
using DotCMIS.Binding;
using CmisSync.Lib.Cmis;
using System.Net;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;


namespace CmisSync.Lib
{
    public class PersistentStandardAuthenticationProvider : StandardAuthenticationProvider, IDisposable
    {
        private IDatabase Db;
        private bool disposed = false;

        public PersistentStandardAuthenticationProvider (IDatabase db)
        {
            if(db == null)
                throw new ArgumentNullException("Given db is null");
            Db = db;
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
                        Db.SetSessionCookies(GetAllCookies(Cookies));
                    }catch(Exception) {}
                }
                disposed = true;
            }
        }


		private static List<Cookie> GetAllCookies(CookieContainer cookieJar)
		{
			CookieCollection cookieCollection = new CookieCollection();

			Hashtable table = (Hashtable) cookieJar.GetType().InvokeMember("m_domainTable",
			                                                               BindingFlags.NonPublic |
			                                                               BindingFlags.GetField |
			                                                               BindingFlags.Instance,
			                                                               null,
			                                                               cookieJar,
			                                                               new object[] {});

			foreach (var tableKey in table.Keys)
			{
				String str_tableKey = (string) tableKey;

				if (str_tableKey[0] == '.')
				{
					str_tableKey = str_tableKey.Substring(1);
				}

				SortedList list = (SortedList) table[tableKey].GetType().InvokeMember("m_list",
				                                                                      BindingFlags.NonPublic |
				                                                                      BindingFlags.GetField |
				                                                                      BindingFlags.Instance,
				                                                                      null,
				                                                                      table[tableKey],
				                                                                      new object[] { });

				foreach (var listKey in list.Keys)
				{
					String url = "https://" + str_tableKey + (string) listKey;
					cookieCollection.Add(cookieJar.GetCookies(new Uri(url)));
				}
			}

			var result = new List<Cookie>();
			foreach(Cookie cookie in cookieCollection)
				result.Add(cookie);
			return result;
		}
    }
}

