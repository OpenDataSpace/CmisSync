//-----------------------------------------------------------------------
// <copyright file="StatusCellRenderer.cs" company="GRAU DATA AG">
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
using Gdk;
using Gtk;

namespace CmisSync.CmisTree
{
    /// <summary>
    /// LoadingStatus cell renderer.
    /// </summary>
    [CLSCompliant(false)]
    public class StatusCellRenderer : Gtk.CellRendererText
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.CmisTree.StatusCellRenderer"/> class with the given foreground color
        /// </summary>
        /// <param name='foreground'>
        /// Foreground.
        /// </param>
        public StatusCellRenderer (Gdk.Color foreground) : base()
        {
            base.ForegroundGdk = foreground;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.CmisTree.StatusCellRenderer"/> class with a default foreground color
        /// </summary>
        public StatusCellRenderer () : base()
        {
            Gdk.Color foreground = new Gdk.Color();
            /// Default color is a light gray
            Gdk.Color.Parse ("#999", ref foreground);
            base.ForegroundGdk = foreground;
        }
    }
}

