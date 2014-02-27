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
	[Register ("GeneralSettingsController")]
	partial class GeneralSettingsController
	{
		[Outlet]
		MonoMac.AppKit.NSView BandwidthTabView { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CancelButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton HelpButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSView ProxyTabView { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton SaveButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTabView TabView { get; set; }

		[Action ("OnCancel:")]
		partial void OnCancel (MonoMac.Foundation.NSObject sender);

		[Action ("OnHelp:")]
		partial void OnHelp (MonoMac.Foundation.NSObject sender);

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

			if (SaveButton != null) {
				SaveButton.Dispose ();
				SaveButton = null;
			}

			if (TabView != null) {
				TabView.Dispose ();
				TabView = null;
			}

			if (ProxyTabView != null) {
				ProxyTabView.Dispose ();
				ProxyTabView = null;
			}

			if (BandwidthTabView != null) {
				BandwidthTabView.Dispose ();
				BandwidthTabView = null;
			}
		}
	}

	[Register ("GeneralSettings")]
	partial class GeneralSettings
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
