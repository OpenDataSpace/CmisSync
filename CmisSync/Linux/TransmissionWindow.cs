//-----------------------------------------------------------------------
// <copyright file="TransmissionWindow.cs" company="GRAU DATA AG">
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

namespace CmisSync {
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.FileTransmission;

    using CmisSync.Widgets;

    [CLSCompliant(false)]
    public partial class TransmissionWindow : Gtk.Window {
        private TransmissionController controller = new TransmissionController();
        private Dictionary<Transmission, TransmissionWidget> widgets = new Dictionary<Transmission, TransmissionWidget>();

        public TransmissionWindow() : base(Gtk.WindowType.Toplevel) {
            this.Build();
            this.HideOnDelete();
            this.Title = Properties_Resources.Transmission;
            this.controller.ShowWindowEvent += () => {
                this.ShowAll();
            };
            this.controller.HideWindowEvent += () => {
                this.Hide();
            };
            this.DeleteEvent += (o, args) => {
                // Do not destroy the window, just hide it
                args.RetVal = true;
                this.Hide();
            };
            this.controller.InsertTransmissionEvent += (Transmission transmission) => {
                Gtk.Application.Invoke(delegate {
                    var widget = new TransmissionWidget() { Transmission = transmission };
                    this.widgets.Add(transmission, widget);
                    this.transmissionList.PackStart(widget, false, true, 2);
                    this.transmissionList.ReorderChild(widget, 0);
                    this.transmissionList.Show();
                });
            };

            this.controller.DeleteTransmissionEvent += (Transmission transmission) => {
                Gtk.Application.Invoke(delegate {
                    var widget = widgets[transmission];
                    widgets.Remove(transmission);
                    this.transmissionList.Remove(widget);
                    this.transmissionList.Show();
                });
            };
        }
    }
}