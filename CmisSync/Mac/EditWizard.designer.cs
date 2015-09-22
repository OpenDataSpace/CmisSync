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
		MonoMac.AppKit.NSTabViewItem BandwidthTabView { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CancelButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTabViewItem CredentialsTab { get; set; }

		[Outlet]
		MonoMac.AppKit.NSBox DownloadLimitBox { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton DownloadLimitCheckBox { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField DownloadLimitTextField { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton FinishButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTabViewItem FolderTab { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField Header { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField LoginStatusLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSProgressIndicator LoginStatusProgress { get; set; }

		[Outlet]
		MonoMac.AppKit.NSOutlineView Outline { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField PasswordLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSecureTextField PasswordText { get; set; }

		[Outlet]
		MonoMac.AppKit.NSImageView SideSplashView { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTabView TabView { get; set; }

		[Outlet]
		MonoMac.AppKit.NSBox UploadLimitBox { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton UploadLimitCheckBox { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField UploadLimitTextField { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField UserLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField UserText { get; set; }

		[Action ("DownloadLimitClicked:")]
		partial void DownloadLimitClicked (MonoMac.Foundation.NSObject sender);

		[Action ("OnCancel:")]
		partial void OnCancel (MonoMac.Foundation.NSObject sender);

		[Action ("OnFinish:")]
		partial void OnFinish (MonoMac.Foundation.NSObject sender);

		[Action ("OnPasswordChanged:")]
		partial void OnPasswordChanged (MonoMac.Foundation.NSObject sender);

		[Action ("UploadLimitClicked:")]
		partial void UploadLimitClicked (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (BandwidthTabView != null) {
				BandwidthTabView.Dispose ();
				BandwidthTabView = null;
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

			if (CredentialsTab != null) {
				CredentialsTab.Dispose ();
				CredentialsTab = null;
			}

			if (DownloadLimitBox != null) {
				DownloadLimitBox.Dispose ();
				DownloadLimitBox = null;
			}

			if (DownloadLimitCheckBox != null) {
				DownloadLimitCheckBox.Dispose ();
				DownloadLimitCheckBox = null;
			}

			if (DownloadLimitTextField != null) {
				DownloadLimitTextField.Dispose ();
				DownloadLimitTextField = null;
			}

			if (FinishButton != null) {
				FinishButton.Dispose ();
				FinishButton = null;
			}

			if (FolderTab != null) {
				FolderTab.Dispose ();
				FolderTab = null;
			}

			if (Header != null) {
				Header.Dispose ();
				Header = null;
			}

			if (LoginStatusLabel != null) {
				LoginStatusLabel.Dispose ();
				LoginStatusLabel = null;
			}

			if (LoginStatusProgress != null) {
				LoginStatusProgress.Dispose ();
				LoginStatusProgress = null;
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

			if (TabView != null) {
				TabView.Dispose ();
				TabView = null;
			}

			if (UploadLimitBox != null) {
				UploadLimitBox.Dispose ();
				UploadLimitBox = null;
			}

			if (UploadLimitCheckBox != null) {
				UploadLimitCheckBox.Dispose ();
				UploadLimitCheckBox = null;
			}

			if (UploadLimitTextField != null) {
				UploadLimitTextField.Dispose ();
				UploadLimitTextField = null;
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
