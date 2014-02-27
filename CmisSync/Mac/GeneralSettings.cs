using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace CmisSync
{
    public partial class GeneralSettings : MonoMac.AppKit.NSWindow
    {
        #region Constructors

        // Called when created from unmanaged code
        public GeneralSettings(IntPtr handle) : base(handle)
        {
            Initialize();
        }
        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public GeneralSettings(NSCoder coder) : base(coder)
        {
            Initialize();
        }
        // Shared initialization code
        void Initialize()
        {
            this.Delegate = new SettingsDelegate();

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

        public class SettingsDelegate : NSWindowDelegate {

            public override bool WindowShouldClose (NSObject sender)
            {
                (sender as GeneralSettings).PerformClose(sender);
                return false;
            }
        }
    }
}

