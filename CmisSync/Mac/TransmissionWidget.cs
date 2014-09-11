
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace CmisSync
{
    public class TransmissionDelegate : NSWindowDelegate
    {
        public override bool WindowShouldClose (NSObject sender)
        {
            (sender as TransmissionWidget).PerformClose (sender);
            return false;
        }
    }

    public partial class TransmissionWidget : MonoMac.AppKit.NSWindow
    {
        #region Constructors

        // Called when created from unmanaged code
        public TransmissionWidget (IntPtr handle) : base (handle)
        {
            Initialize ();
        }
        
        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public TransmissionWidget (NSCoder coder) : base (coder)
        {
            Initialize ();
        }
        
        // Shared initialization code
        void Initialize ()
        {
            Delegate = new TransmissionDelegate ();
        }

        #endregion

        public override void OrderFrontRegardless ()
        {
            NSApplication.SharedApplication.AddWindowsItem (this, Properties_Resources.ApplicationName, false);
            NSApplication.SharedApplication.ActivateIgnoringOtherApps (true);
            MakeKeyAndOrderFront (this);

            if (Program.UI != null)
                Program.UI.UpdateDockIconVisibility ();

            base.OrderFrontRegardless ();
        }

        public override void PerformClose (NSObject sender)
        {
            base.OrderOut (this);
            NSApplication.SharedApplication.RemoveWindowsItem (this);

            if (Program.UI != null)
                Program.UI.UpdateDockIconVisibility ();

            return;
        }
    }
}

