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
	[Register ("TransmissionWidgetController")]
	partial class TransmissionWidgetController
	{
		[Outlet]
		MonoMac.AppKit.NSButton FinishButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableColumn TableColumnPath { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableColumn TableColumnProgress { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableColumn TableColumnRepo { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableColumn TableColumnStatus { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableColumn TableColumnUpdateTime { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenu TableRowContextMenu { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem TableRowMenuOpen { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableView TableView { get; set; }

		[Action ("OnFinish:")]
		partial void OnFinish (MonoMac.Foundation.NSObject sender);

		[Action ("OnOpen:")]
		partial void OnOpen (MonoMac.Foundation.NSObject sender);

		[Action ("OnOpenLocation:")]
		partial void OnOpenLocation (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (FinishButton != null) {
				FinishButton.Dispose ();
				FinishButton = null;
			}

			if (TableColumnPath != null) {
				TableColumnPath.Dispose ();
				TableColumnPath = null;
			}

			if (TableColumnProgress != null) {
				TableColumnProgress.Dispose ();
				TableColumnProgress = null;
			}

			if (TableColumnRepo != null) {
				TableColumnRepo.Dispose ();
				TableColumnRepo = null;
			}

			if (TableColumnStatus != null) {
				TableColumnStatus.Dispose ();
				TableColumnStatus = null;
			}

			if (TableColumnUpdateTime != null) {
				TableColumnUpdateTime.Dispose ();
				TableColumnUpdateTime = null;
			}

			if (TableRowContextMenu != null) {
				TableRowContextMenu.Dispose ();
				TableRowContextMenu = null;
			}

			if (TableRowMenuOpen != null) {
				TableRowMenuOpen.Dispose ();
				TableRowMenuOpen = null;
			}

			if (TableView != null) {
				TableView.Dispose ();
				TableView = null;
			}
		}
	}

	[Register ("TransmissionWidget")]
	partial class TransmissionWidget
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
