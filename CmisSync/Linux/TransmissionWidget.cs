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

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Widgets.TransmissionWidget"/> class.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the monitored transmission.
        /// </summary>
        /// <value>The transmission.</value>
        public Transmission Transmission {
            get {
                return this.transmission;
            }

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

                    this.repoLabel.Markup = string.Format("<small>{0}</small>", this.transmission.Repository);
                    this.fileNameLabel.Text = System.IO.Path.GetFileName(this.transmission.Path);
                    this.fileNameLabel.TooltipText = this.transmission.Path;
                    this.lastModificationLabel.Markup = string.Format("<small>{0}</small>", transmission.LastModification.ToString());
                    this.openFileInFolderButton.Clicked += (object s, EventArgs e) => {
                        Utils.OpenFolder(new FileInfo(this.transmission.Path).Directory.FullName);
                    };
                    this.UpdateStatus(this.transmission.Status);
                    this.UpdateSizeAndPositionStatus(this.transmission);
                    switch (this.transmission.Type) {
                    case TransmissionType.DOWNLOAD_NEW_FILE:
                        this.fileTypeImage.Pixbuf = UIHelpers.GetIcon("dataspacesync-downloading", 16);
                        break;
                    case TransmissionType.UPLOAD_NEW_FILE:
                        this.fileTypeImage.Pixbuf = UIHelpers.GetIcon("dataspacesync-uploading", 16);
                        break;
                    default:
                        this.fileTypeImage.Pixbuf = UIHelpers.GetIcon("dataspacesync-updating", 16);
                        break;
                    }
                }
            }
        }

        private double Progress {
            get {
                return this.transmissionProgressBar.Fraction;
            }

            set {
                this.transmissionProgressBar.Fraction = value;
                this.transmissionProgressBar.Text = CmisSync.Lib.Utils.FormatPercent(value * 100);
            }
        }

        private void TransmissionUpdate(object sender, PropertyChangedEventArgs args) {
            var t = sender as Transmission;
            if (args.PropertyName == CmisSync.Lib.Utils.NameOf(() => t.BitsPerSecond)) {
                Gtk.Application.Invoke(delegate {
                    this.bandwidthLabel.Markup = string.Format("<small>{0}</small>", CmisSync.Lib.Utils.FormatBandwidth(t.BitsPerSecond.GetValueOrDefault()));
                    this.UpdateSizeAndPositionStatus(t);
                });
            } else if (args.PropertyName == CmisSync.Lib.Utils.NameOf(() => t.LastModification)) {
                Gtk.Application.Invoke(delegate {
                    this.lastModificationLabel.Markup = string.Format("<small>{0}</small>", t.LastModification.ToString());
                });
            } else if (args.PropertyName == CmisSync.Lib.Utils.NameOf(() => t.Length)) {
                Gtk.Application.Invoke(delegate {
                    this.UpdateSizeAndPositionStatus(t);
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

        private void UpdateSizeAndPositionStatus(Transmission t) {
            string pos = t.Position != null && t.Position != t.Length ? CmisSync.Lib.Utils.FormatSize(t.Position.GetValueOrDefault()) + "/" : string.Empty;
            string size = t.Length != null ? CmisSync.Lib.Utils.FormatSize(t.Length.GetValueOrDefault()) : string.Empty;
            this.statusDetailsLabel.Markup = string.Format("<small>{0}{1}</small>", pos, size);
        }

        private void UpdateStatus(TransmissionStatus status) {
            this.animation.Stop();
            switch (status) {
            case TransmissionStatus.FINISHED:
                this.openFileInFolderButton.Sensitive = true;
                this.Progress = 1.0;
                this.bandwidthLabel.Markup = string.Empty;
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