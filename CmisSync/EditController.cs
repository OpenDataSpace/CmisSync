//-----------------------------------------------------------------------
// <copyright file="EditController.cs" company="GRAU DATA AG">
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
using System;

namespace CmisSync
{
    /// <summary>
    /// Controller for the Edit diaglog.
    /// </summary>
    public class EditController
    {
        //===== Actions =====
        /// <summary>
        /// Open Edit Window Action
        /// </summary>
        public event Action OpenWindowEvent = delegate { };
        /// <summary>
        /// Save Folder Action
        /// </summary>
        public event Action SaveFolderEvent = delegate { };
        /// <summary>
        /// Close Edit Window Action
        /// </summary>
        public event Action CloseWindowEvent = delegate { };

        /// <summary>
        /// Show Edit Window
        /// </summary>
        public void OpenWindow()
        {
            OpenWindowEvent();
        }

        /// <summary>
        /// Save Folder
        /// </summary>
        public void SaveFolder()
        {
            SaveFolderEvent();
        }

        /// <summary>
        /// Close Edit Window
        /// </summary>
        public void CloseWindow()
        {
            CloseWindowEvent();
        }
    }
}
