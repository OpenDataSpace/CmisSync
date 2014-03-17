// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;
using System.CodeDom.Compiler;

namespace CmisSync
{
	[Register ("ProxySettingsController")]
	partial class ProxySettingsController
	{
		[Outlet]
		MonoMac.AppKit.NSButton CredentialsCheckbox { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButtonCell CustomProxyButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButtonCell DefaultProxyButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButtonCell NoProxyButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField PasswordLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSecureTextField PasswordTextField { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField ServerLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField ServerTextField { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField UsernameLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField UsernameTextField { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ServerLabel != null) {
				ServerLabel.Dispose ();
				ServerLabel = null;
			}

			if (CredentialsCheckbox != null) {
				CredentialsCheckbox.Dispose ();
				CredentialsCheckbox = null;
			}

			if (UsernameLabel != null) {
				UsernameLabel.Dispose ();
				UsernameLabel = null;
			}

			if (PasswordLabel != null) {
				PasswordLabel.Dispose ();
				PasswordLabel = null;
			}

			if (UsernameTextField != null) {
				UsernameTextField.Dispose ();
				UsernameTextField = null;
			}

			if (PasswordTextField != null) {
				PasswordTextField.Dispose ();
				PasswordTextField = null;
			}

			if (ServerTextField != null) {
				ServerTextField.Dispose ();
				ServerTextField = null;
			}

			if (NoProxyButton != null) {
				NoProxyButton.Dispose ();
				NoProxyButton = null;
			}

			if (DefaultProxyButton != null) {
				DefaultProxyButton.Dispose ();
				DefaultProxyButton = null;
			}

			if (CustomProxyButton != null) {
				CustomProxyButton.Dispose ();
				CustomProxyButton = null;
			}
		}
	}

	[Register ("ProxySettings")]
	partial class ProxySettings
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
