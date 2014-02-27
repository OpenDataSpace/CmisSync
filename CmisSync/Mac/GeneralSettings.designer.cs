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
	[Register ("GeneralSettings")]
	partial class GeneralSettings
	{
		[Outlet]
		MonoMac.AppKit.NSButton CancelButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton HelpButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButtonCell ManualProxyButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButtonCell NoProxyButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSecureTextField ProxyPassword { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField ProxyPasswordLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField ProxyServer { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField ProxyServerLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTabViewItem ProxyTab { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField ProxyUsername { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField ProxyUsernameLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButtonCell RequiresAuthorizationCheckBox { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton SaveButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButtonCell SystemDefaultProxyButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTabView TabView { get; set; }

		[Action ("OnCancel:")]
		partial void OnCancel (MonoMac.Foundation.NSObject sender);

		[Action ("OnDefaultProxy:")]
		partial void OnDefaultProxy (MonoMac.Foundation.NSObject sender);

		[Action ("OnHelp:")]
		partial void OnHelp (MonoMac.Foundation.NSObject sender);

		[Action ("OnManualProxy:")]
		partial void OnManualProxy (MonoMac.Foundation.NSObject sender);

		[Action ("OnNoProxy:")]
		partial void OnNoProxy (MonoMac.Foundation.NSObject sender);

		[Action ("OnRequireAuth:")]
		partial void OnRequireAuth (MonoMac.Foundation.NSObject sender);

		[Action ("OnSave:")]
		partial void OnSave (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (CancelButton != null) {
				CancelButton.Dispose ();
				CancelButton = null;
			}

			if (HelpButton != null) {
				HelpButton.Dispose ();
				HelpButton = null;
			}

			if (ManualProxyButton != null) {
				ManualProxyButton.Dispose ();
				ManualProxyButton = null;
			}

			if (NoProxyButton != null) {
				NoProxyButton.Dispose ();
				NoProxyButton = null;
			}

			if (ProxyPassword != null) {
				ProxyPassword.Dispose ();
				ProxyPassword = null;
			}

			if (ProxyPasswordLabel != null) {
				ProxyPasswordLabel.Dispose ();
				ProxyPasswordLabel = null;
			}

			if (ProxyServer != null) {
				ProxyServer.Dispose ();
				ProxyServer = null;
			}

			if (ProxyServerLabel != null) {
				ProxyServerLabel.Dispose ();
				ProxyServerLabel = null;
			}

			if (ProxyTab != null) {
				ProxyTab.Dispose ();
				ProxyTab = null;
			}

			if (ProxyUsername != null) {
				ProxyUsername.Dispose ();
				ProxyUsername = null;
			}

			if (ProxyUsernameLabel != null) {
				ProxyUsernameLabel.Dispose ();
				ProxyUsernameLabel = null;
			}

			if (RequiresAuthorizationCheckBox != null) {
				RequiresAuthorizationCheckBox.Dispose ();
				RequiresAuthorizationCheckBox = null;
			}

			if (SaveButton != null) {
				SaveButton.Dispose ();
				SaveButton = null;
			}

			if (SystemDefaultProxyButton != null) {
				SystemDefaultProxyButton.Dispose ();
				SystemDefaultProxyButton = null;
			}

			if (TabView != null) {
				TabView.Dispose ();
				TabView = null;
			}
		}
	}

	[Register ("GeneralSettingsController")]
	partial class GeneralSettingsController
	{
		[Action ("OnCancel:")]
		partial void OnCancel (MonoMac.Foundation.NSObject sender);

		[Action ("OnHelp:")]
		partial void OnHelp (MonoMac.Foundation.NSObject sender);

		[Action ("OnSave:")]
		partial void OnSave (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
