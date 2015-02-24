//-----------------------------------------------------------------------
// <copyright file="TransmissionWidget.cs" company="GRAU DATA AG">
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

namespace CmisSync.Widgets {
    using System;
    using System.IO;

    using CmisSync.Lib.Events;

    [System.ComponentModel.ToolboxItem(true)]
    public partial class TransmissionWidget : Gtk.Bin {
        private FileTransmissionEvent transmission;
        public TransmissionWidget() {
            this.Build();
/*            this.transmission = transmission;
            this.transmissionProgressBar.Pulse();
            this.transmission.TransmissionStatus += (object sender, TransmissionProgressEventArgs e) => {
                if (e.Percent != null) {
                    this.transmissionProgressBar.Fraction = e.Percent.GetValueOrDefault() / 100;
                }
            };
            */
        }
    }
}