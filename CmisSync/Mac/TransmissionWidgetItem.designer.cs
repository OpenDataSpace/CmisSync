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
	[Register ("TransmissionWidgetItem")]
	partial class TransmissionWidgetItem
	{
		[Outlet]
		public MonoMac.AppKit.NSProgressIndicator progress { get; private set; }
		
        [Outlet]
        public MonoMac.AppKit.NSTextField labelName { get; private set; }

        [Outlet]
        public MonoMac.AppKit.NSTextField labelStatus { get; private set; }

        [Outlet]
        public MonoMac.AppKit.NSTextField labelDate { get; private set; }

		void ReleaseDesignerOutlets ()
		{
			if (progress != null) {
				progress.Dispose ();
				progress = null;
			}
            if (labelName != null) {
                labelName.Dispose ();
                labelName = null;
            }
            if (labelStatus != null) {
                labelStatus.Dispose ();
                labelStatus = null;
            }
            if (labelDate != null) {
                labelDate.Dispose ();
                labelDate = null;
            }
		}
	}
}
