//-----------------------------------------------------------------------
// <copyright file="BandwidthSettingsWidget.cs" company="GRAU DATA AG">
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
﻿
namespace CmisSync.Widgets {
    using System;
    using System.ComponentModel;

    [ToolboxItem(true)]
    [CLSCompliant(false)]
    public partial class BandwidthSettingsWidget : Gtk.Bin {
        public event EventHandler Changed;
        public BandwidthSettingsWidget() {
            this.Build();
            this.downloadLimitWidget.Changed += this.OnChange;
            this.uploadLimitWidget.Changed += this.OnChange;
        }

        public long UploadLimit {
            get {
                return (long)this.uploadLimitWidget.Limit * 1024;
            }

            set {
                this.uploadLimitWidget.Limit = value / 1024;
            }
        }

        public long DownloadLimit {
            get {
                return (long)this.downloadLimitWidget.Limit * 1024;
            }

            set {
                this.downloadLimitWidget.Limit = value / 1024;
            }
        }

        private void OnChange(object sender, EventArgs args) {
            var handler = this.Changed;
            if (handler != null) {
                handler(this, args);
            }
        }
    }
}