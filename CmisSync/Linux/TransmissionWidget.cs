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
    using System.ComponentModel;
    using System.IO;
    using System.Timers;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.FileTransmission;

    [System.ComponentModel.ToolboxItem(true)]
    public partial class TransmissionWidget : Gtk.Bin {
        private Transmission transmission;

        private Timer animation;

        public TransmissionWidget() {
            this.Build();
            this.animation = new Timer() {
                Interval = 100
            };

            this.animation.Elapsed += delegate {
                Gtk.Application.Invoke(delegate {
                    this.transmissionProgressBar.Pulse();
                });
            };
        }

        public Transmission Transmission {
            set {
                if (this.transmission != value) {
                    if (this.transmission != null) {
                        this.transmission.PropertyChanged -= this.TransmissionUpdate;
                    }

                    this.transmission = value;
                    this.transmission.PropertyChanged += this.TransmissionUpdate;
                    if (this.transmission.Percent != null) {
                        this.Progress = this.transmission.Percent.GetValueOrDefault() / 100;
                    }

                    this.repoLabel.Text = this.transmission.Repository;
                    this.fileNameLabel.Text = System.IO.Path.GetFileName(this.transmission.Path);
                    this.openFileInFolderButton.Activated += (object s, EventArgs e) => {
                        Utils.OpenFolder(System.IO.Path.GetDirectoryName(this.transmission.Path));
                    };
                    this.UpdateStatus(this.transmission.Status);
                }
            }

            get {
                return this.transmission;
            }
        }

        private void TransmissionUpdate(object sender, PropertyChangedEventArgs args) {
            var t = sender as Transmission;
            if (args.PropertyName == CmisSync.Lib.Utils.NameOf(() => t.BitsPerSecond)) {
                Gtk.Application.Invoke(delegate {
                    this.bandwidthLabel.Text = CmisSync.Lib.Utils.FormatBandwidth(t.BitsPerSecond.GetValueOrDefault());
                });
            } else if (args.PropertyName == CmisSync.Lib.Utils.NameOf(() => t.LastModification)) {
                Gtk.Application.Invoke(delegate {
                    this.lastModificationLabel.Text = t.LastModification.ToString();
                });
            } else if (args.PropertyName == CmisSync.Lib.Utils.NameOf(() => t.Length)) {
                Gtk.Application.Invoke(delegate {

                });
            } else if (args.PropertyName == CmisSync.Lib.Utils.NameOf(() => t.Percent)) {
                var progress = t.Percent;
                if (progress != null) {
                    Gtk.Application.Invoke(delegate {
                        this.Progress = progress.GetValueOrDefault() / 100;
                    });
                }
            } else if (args.PropertyName == CmisSync.Lib.Utils.NameOf(() => t.Status)) {
                Gtk.Application.Invoke(delegate {
                    this.UpdateStatus(t.Status);
                });
            }
        }

        public double Progress {
            get {
                return this.transmissionProgressBar.Fraction;
            }

            set {
                this.transmissionProgressBar.Fraction = value;
                this.transmissionProgressBar.Text = CmisSync.Lib.Utils.FormatPercent(value * 100);
            }
        }

        private void UpdateStatus(TransmissionStatus status) {
            this.animation.Stop();
            switch (status) {
            case TransmissionStatus.FINISHED:
                this.transmissionProgressBar.Visible = false;
                this.openFileInFolderButton.Sensitive = true;
                this.Progress = 1.0;
                break;
            case TransmissionStatus.ABORTING:
                this.animation.Start();
                break;
            default:
                this.openFileInFolderButton.Sensitive = false;
                break;
            }
        }
    }
}