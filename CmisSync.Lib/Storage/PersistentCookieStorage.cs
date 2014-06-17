//-----------------------------------------------------------------------
// <copyright file="PersistentCookieStorage.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Storage
{
    using System;
    using System.Net;

    using DBreeze;
    using DBreeze.DataTypes;

    /// <summary>
    /// Persistent cookie storage. Saves the cookie collection into the given dbreeze instance
    /// </summary>
    public class PersistentCookieStorage : ICookieStorage
    {
        private static readonly string CookieTable = "cookies";
        private DBreezeEngine db;

        static PersistentCookieStorage()
        {
            DBreezeInitializerSingleton.Init();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Storage.PersistentCookieStorage"/> class.
        /// </summary>
        /// <param name='db'>
        /// DBreeze engine instance to be used for saving collection.
        /// </param>
        [CLSCompliant(false)]
        public PersistentCookieStorage(DBreezeEngine db)
        {
            if(db == null)
            {
                throw new ArgumentNullException("Given db engine is null");
            }

            this.db = db;
        }

        /// <summary>
        /// Gets or sets the cookie collection.
        /// </summary>
        /// <value>
        /// The cookies.
        /// </value>
        public CookieCollection Cookies
        {
            get
            {
                CookieCollection cookies = new CookieCollection();
                using(var tran = this.db.GetTransaction())
                {
                    foreach (var row in tran.SelectForward<int, DbCustomSerializer<Cookie>>(CookieTable))
                    {
                        var value = row.Value;
                        if(value == null)
                        {
                            continue;
                        }

                        var cookie = value.Get;
                        if(cookie == null)
                        {
                            continue;
                        }

                        if(!cookie.Expired)
                        {
                            cookies.Add(cookie);
                        }
                    }
                }

                return cookies;
            }

            set
            {
                using(var tran = this.db.GetTransaction())
                {
                    if(value == null)
                    {
                        tran.RemoveAllKeys(CookieTable, false);
                    }
                    else
                    {
                        int i = 0;
                        foreach(Cookie cookie in value)
                        {
                            if(!cookie.Expired)
                            {
                                tran.Insert<int, DbCustomSerializer<Cookie>>(CookieTable, i, cookie);
                            }

                            i++;
                        }
                    }

                    tran.Commit();
                }
            }
        }
    }
}
