//-----------------------------------------------------------------------
// <copyright file="NodeModelUtils.cs" company="GRAU DATA AG">
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
using CmisSync.Lib.Cmis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CmisSync
{
    namespace CmisTree
    {
        /// <summary>
        /// Data Model Utilities for the WPF UI representing a CMIS repository
        /// </summary>
        public static class NodeModelUtils
        {
            /// <summary>
            /// Get the ignored folder list
            /// </summary>
            /// <returns></returns>
            public static List<string> GetIgnoredFolder(RootFolder repo)
            {
                List<string> result = new List<string>();
                foreach (Folder child in repo.Children)
                {
                    result.AddRange(Folder.GetIgnoredFolder(child));
                }
                return result;
            }

            /// <summary>
            /// Get the selected folder list
            /// </summary>
            /// <returns></returns>
            public static List<string> GetSelectedFolder(RootFolder repo)
            {
                List<string> result = new List<string>();
                foreach (Folder child in repo.Children)
                {
                    result.AddRange(Folder.GetSelectedFolder(child));
                }
                return result;
            }
        }
    }
}
