//-----------------------------------------------------------------------
// <copyright file="BandwidthLimitWidget.cs" company="GRAU DATA AG">
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
    public partial class BandwidthLimitWidget : Gtk.Bin {
        public event EventHandler Changed;

        public BandwidthLimitWidget() {
            this.Build();
        }

        public double Limit {
            get {
                if (this.activateLimitToggle.Active) {
                    return this.bandwidthSpinButton.Value;
                } else {
                    return 0;
                }
            }

            set {
                this.activateLimitToggle.Active = value > 0;
                this.bandwidthSpinButton.Sensitive = value > 0;
                this.bandwidthSpinButton.Value = value > 0 ? value : 100;
            }
        }

        protected void IsLimitedToggled(object sender, EventArgs e) {
            this.bandwidthSpinButton.Sensitive = this.activateLimitToggle.Active;
            this.ValueChanged(sender, e);
        }

        protected void ValueChanged(object sender, EventArgs e) {
            var handler = this.Changed;
            if (handler != null) {
                handler(this, e);
            }
        }
    }
}