
// This file has been generated by the GUI designer. Do not modify.
namespace CmisSync
{
	public partial class CredentialsWidget
	{
		private global::Gtk.VBox vbox1;
		private global::Gtk.Label addressLabel;
		private global::CmisSync.Widgets.UrlWidget urlWidget;
		private global::Gtk.HBox hbox3;
		private global::Gtk.VBox vbox6;
		private global::Gtk.Label userNameLabel;
		private global::Gtk.Entry usernameEntry;
		private global::Gtk.VBox vbox7;
		private global::Gtk.Label passwordLabel;
		private global::Gtk.Entry passwordEntry;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget CmisSync.CredentialsWidget
			global::Stetic.BinContainer.Attach (this);
			this.Name = "CmisSync.CredentialsWidget";
			// Container child CmisSync.CredentialsWidget.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox ();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.addressLabel = new global::Gtk.Label ();
			this.addressLabel.Name = "addressLabel";
			this.addressLabel.Xalign = 0F;
			this.addressLabel.LabelProp = global::Mono.Unix.Catalog.GetString ("label1");
			this.vbox1.Add (this.addressLabel);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.addressLabel]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.urlWidget = new global::CmisSync.Widgets.UrlWidget ();
			this.urlWidget.Events = ((global::Gdk.EventMask)(256));
			this.urlWidget.Name = "urlWidget";
			this.urlWidget.IsUrlEditable = false;
			this.urlWidget.ValidationActivated = false;
			this.vbox1.Add (this.urlWidget);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.urlWidget]));
			w2.Position = 1;
			w2.Expand = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox3 = new global::Gtk.HBox ();
			this.hbox3.Name = "hbox3";
			this.hbox3.Spacing = 6;
			// Container child hbox3.Gtk.Box+BoxChild
			this.vbox6 = new global::Gtk.VBox ();
			this.vbox6.Name = "vbox6";
			this.vbox6.Spacing = 6;
			// Container child vbox6.Gtk.Box+BoxChild
			this.userNameLabel = new global::Gtk.Label ();
			this.userNameLabel.Name = "userNameLabel";
			this.userNameLabel.Xalign = 0F;
			this.userNameLabel.LabelProp = global::Mono.Unix.Catalog.GetString ("Username");
			this.vbox6.Add (this.userNameLabel);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox6 [this.userNameLabel]));
			w3.Position = 0;
			w3.Fill = false;
			// Container child vbox6.Gtk.Box+BoxChild
			this.usernameEntry = new global::Gtk.Entry ();
			this.usernameEntry.Sensitive = false;
			this.usernameEntry.CanFocus = true;
			this.usernameEntry.Name = "usernameEntry";
			this.usernameEntry.IsEditable = false;
			this.usernameEntry.InvisibleChar = '•';
			this.vbox6.Add (this.usernameEntry);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.vbox6 [this.usernameEntry]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			this.hbox3.Add (this.vbox6);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox3 [this.vbox6]));
			w5.Position = 0;
			// Container child hbox3.Gtk.Box+BoxChild
			this.vbox7 = new global::Gtk.VBox ();
			this.vbox7.Name = "vbox7";
			this.vbox7.Spacing = 6;
			// Container child vbox7.Gtk.Box+BoxChild
			this.passwordLabel = new global::Gtk.Label ();
			this.passwordLabel.Name = "passwordLabel";
			this.passwordLabel.Xalign = 0F;
			this.passwordLabel.LabelProp = global::Mono.Unix.Catalog.GetString ("Password");
			this.vbox7.Add (this.passwordLabel);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox7 [this.passwordLabel]));
			w6.Position = 0;
			w6.Fill = false;
			// Container child vbox7.Gtk.Box+BoxChild
			this.passwordEntry = new global::Gtk.Entry ();
			this.passwordEntry.CanFocus = true;
			this.passwordEntry.Name = "passwordEntry";
			this.passwordEntry.IsEditable = true;
			this.passwordEntry.Visibility = false;
			this.passwordEntry.InvisibleChar = '•';
			this.vbox7.Add (this.passwordEntry);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox7 [this.passwordEntry]));
			w7.Position = 1;
			w7.Expand = false;
			w7.Fill = false;
			this.hbox3.Add (this.vbox7);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.hbox3 [this.vbox7]));
			w8.Position = 1;
			this.vbox1.Add (this.hbox3);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.hbox3]));
			w9.Position = 2;
			w9.Expand = false;
			w9.Fill = false;
			this.Add (this.vbox1);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Hide ();
			this.urlWidget.Changed += new global::System.EventHandler (this.OnUrlWidgetChanged);
			this.passwordEntry.Changed += new global::System.EventHandler (this.OnPasswordChanged);
		}
	}
}
