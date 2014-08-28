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
		MonoMac.AppKit.NSTableView TableView { get; set; }

		[Action ("OnFinish:")]
		partial void OnFinish (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (TableView != null) {
				TableView.Dispose ();
				TableView = null;
			}

			if (FinishButton != null) {
				FinishButton.Dispose ();
				FinishButton = null;
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
