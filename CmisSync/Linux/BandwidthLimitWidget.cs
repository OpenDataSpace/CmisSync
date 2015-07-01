
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

        public long Limit {
            get {
                if (this.activateLimitToggle.Active) {
                    return this.bandwidthSpinButton.ValueAsInt;
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