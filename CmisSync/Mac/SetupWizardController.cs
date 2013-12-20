using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace CmisSync
{
    public partial class SetupWizardController : MonoMac.AppKit.NSWindowController
    {

        #region Constructors

        // Called when created from unmanaged code
        public SetupWizardController (IntPtr handle) : base (handle)
        {
            Initialize ();
        }
        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public SetupWizardController (NSCoder coder) : base (coder)
        {
            Initialize ();
        }
        // Call to load from the XIB/NIB file
        public SetupWizardController () : base ("SetupWizard")
        {
            Initialize ();
        }
        // Shared initialization code
        void Initialize ()
        {
            Controller = new SetupController ();

            Controller.ShowWindowEvent += delegate {
                InvokeOnMainThread (delegate {
                    Window.OrderFrontRegardless();
                });
            };
        }

        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();

            this.SideSplashView.Image = new NSImage (Path.Combine (NSBundle.MainBundle.ResourcePath, "Pixmaps", "side-splash.png")) {
                Size = new SizeF (150, 482)
            };
        }

        #endregion

        //strongly typed window accessor
        public new SetupWizard Window {
            get {
                return (SetupWizard)base.Window;
            }
        }

        private SetupController Controller;

        void ShowLoginPage()
        {
            Header.StringValue = CmisSync.Properties_Resources.Where;
            Description.StringValue = "";
        }

    }
}

