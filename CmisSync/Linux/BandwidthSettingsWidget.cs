
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
                return this.uploadLimitWidget.Limit * 1024;
            }

            set {
                this.uploadLimitWidget.Limit = value / 1024;
            }
        }

        public long DownloadLimit {
            get {
                return this.downloadLimitWidget.Limit * 1024;
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