
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