
// This file has been generated by the GUI designer. Do not modify.
namespace CmisSync.Widgets
{
	public partial class UrlWidget
	{
		private global::Gtk.VBox vbox1;
		private global::Gtk.Label titleLabel;
		private global::Gtk.Entry urlEntry;
		private global::Gtk.Label msgLabel;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget CmisSync.Widgets.UrlWidget
			global::Stetic.BinContainer.Attach (this);
			this.Name = "CmisSync.Widgets.UrlWidget";
			// Container child CmisSync.Widgets.UrlWidget.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox ();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.titleLabel = new global::Gtk.Label ();
			this.titleLabel.Name = "titleLabel";
			this.titleLabel.Xalign = 0F;
			this.titleLabel.LabelProp = global::Mono.Unix.Catalog.GetString ("URL");
			this.titleLabel.SingleLineMode = true;
			this.vbox1.Add (this.titleLabel);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.titleLabel]));
			w1.Position = 0;
			w1.Expand = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.urlEntry = new global::Gtk.Entry ();
			this.urlEntry.CanFocus = true;
			this.urlEntry.Name = "urlEntry";
			this.urlEntry.Text = global::Mono.Unix.Catalog.GetString ("https://");
			this.urlEntry.IsEditable = true;
			this.urlEntry.InvisibleChar = '•';
			this.vbox1.Add (this.urlEntry);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.urlEntry]));
			w2.Position = 1;
			w2.Expand = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.msgLabel = new global::Gtk.Label ();
			this.msgLabel.Name = "msgLabel";
			this.msgLabel.Xalign = 0F;
			this.msgLabel.Wrap = true;
			this.msgLabel.Selectable = true;
			this.vbox1.Add (this.msgLabel);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.msgLabel]));
			w3.Position = 2;
			this.Add (this.vbox1);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Hide ();
			this.urlEntry.Changed += new global::System.EventHandler (this.ValidateUrl);
		}
	}
}