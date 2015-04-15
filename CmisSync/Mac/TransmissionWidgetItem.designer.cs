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
        public MonoMac.AppKit.NSTextField labelHead { get; private set; }

        [Outlet]
        public MonoMac.AppKit.NSTextField labelFoot { get; private set; }

		void ReleaseDesignerOutlets ()
		{
			if (progress != null) {
				progress.Dispose ();
				progress = null;
			}
            if (labelHead != null) {
                labelHead.Dispose ();
                labelHead = null;
            }
            if (labelFoot != null) {
                labelFoot.Dispose ();
                labelFoot = null;
            }
		}
	}
}
