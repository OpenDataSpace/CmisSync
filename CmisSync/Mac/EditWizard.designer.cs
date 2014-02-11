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
	[Register ("EditWizardController")]
	partial class EditWizardController
	{
		[Outlet]
		MonoMac.AppKit.NSTextField AddressLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField AddressText { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CancelButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTabViewItem CredentialsTab { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton FinishButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTabViewItem FolderTab { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField Header { get; set; }

		[Outlet]
		MonoMac.AppKit.NSOutlineView Outline { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField PasswordLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSecureTextField PasswordText { get; set; }

		[Outlet]
		MonoMac.AppKit.NSImageView SideSplashView { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField UserLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField UserText { get; set; }

		[Action ("OnCancel:")]
		partial void OnCancel (MonoMac.Foundation.NSObject sender);

		[Action ("OnFinish:")]
		partial void OnFinish (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (FolderTab != null) {
				FolderTab.Dispose ();
				FolderTab = null;
			}

			if (CredentialsTab != null) {
				CredentialsTab.Dispose ();
				CredentialsTab = null;
			}

			if (AddressLabel != null) {
				AddressLabel.Dispose ();
				AddressLabel = null;
			}

			if (AddressText != null) {
				AddressText.Dispose ();
				AddressText = null;
			}

			if (CancelButton != null) {
				CancelButton.Dispose ();
				CancelButton = null;
			}

			if (FinishButton != null) {
				FinishButton.Dispose ();
				FinishButton = null;
			}

			if (Header != null) {
				Header.Dispose ();
				Header = null;
			}

			if (Outline != null) {
				Outline.Dispose ();
				Outline = null;
			}

			if (PasswordLabel != null) {
				PasswordLabel.Dispose ();
				PasswordLabel = null;
			}

			if (PasswordText != null) {
				PasswordText.Dispose ();
				PasswordText = null;
			}

			if (SideSplashView != null) {
				SideSplashView.Dispose ();
				SideSplashView = null;
			}

			if (UserLabel != null) {
				UserLabel.Dispose ();
				UserLabel = null;
			}

			if (UserText != null) {
				UserText.Dispose ();
				UserText = null;
			}
		}
	}

	[Register ("EditWizard")]
	partial class EditWizard
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
