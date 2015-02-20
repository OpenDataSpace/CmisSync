
namespace CmisSync {
    using System;

    public partial class TransmissionWindow : Gtk.Window {
        private TransmissionController controller = new TransmissionController();
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
        }
    }
}