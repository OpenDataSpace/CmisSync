// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;
using System.CodeDom.Compiler;

namespace DiagnoseTool
{
	[Register ("MainWindow")]
	partial class MainWindow
	{
		[Outlet]
		MonoMac.AppKit.NSPopUpButtonCell folderSelection { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField output { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton RunButton { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (folderSelection != null) {
				folderSelection.Dispose ();
				folderSelection = null;
			}

			if (output != null) {
				output.Dispose ();
				output = null;
			}

			if (RunButton != null) {
				RunButton.Dispose ();
				RunButton = null;
			}
		}
	}

	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
