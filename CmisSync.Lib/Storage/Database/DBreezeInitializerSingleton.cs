//-----------------------------------------------------------------------
// <copyright file="DBreezeInitializerSingleton.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib
{
    using System;

    using Newtonsoft.Json;

    /// <summary>
    /// DBreeze initializer should be called by every class before using DBreeze
    /// </summary>
    public sealed class DBreezeInitializerSingleton
    {
        private static volatile DBreezeInitializerSingleton instance;
        private static object syncRoot = new object();

        private DBreezeInitializerSingleton()
        {
            // Use Newtonsoft.Json as Serializator
            DBreeze.Utils.CustomSerializator.Serializator = JsonConvert.SerializeObject; 
            DBreeze.Utils.CustomSerializator.Deserializator = JsonConvert.DeserializeObject;
        }

        /// <summary>
        /// Initializes DBreeze static classes
        /// </summary>
        public static void Init()
        {
            if (instance == null) 
            {
                lock (syncRoot) 
                {
                    if (instance == null) 
                    {
                        instance = new DBreezeInitializerSingleton();
                    }                        
                }
            }
        }
    }
}