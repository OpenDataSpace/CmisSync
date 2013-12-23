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

            Controller.HideWindowEvent += delegate {
                InvokeOnMainThread (delegate {
                    Window.PerformClose (this);
                });
            };

            Controller.ChangePageEvent += delegate (PageType type) {
                using (var a = new NSAutoreleasePool ())
                {
                    if (SubController != null)
                    {
                        SubController.Dispose ();
                        SubController = null;
                    }

                    LoadWindow();
                    InvokeOnMainThread (delegate {
                        switch (type)
                        {
                        case PageType.Setup:
//                            ShowWelcomePage();
                            break;
                        case PageType.Tutorial:
//                            ShowTutorialPage();
                            break;
                        case PageType.Add1:
                            ShowLoginPage();
                            break;
                        case PageType.Add2:
                            ShowRepoSelectPage();
                            break;
                        case PageType.Customize:
//                            ShowCustomizePage();
                            break;
                        case PageType.Syncing:
//                            ShowSyncingPage();
                            break;
                        case PageType.Finished:
//                            ShowFinishedPage();
                            break;
                        }
                    });
                }
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
        private NSViewController SubController;

        void ShowLoginPage()
        {
            Header.StringValue = Properties_Resources.Where;
            Description.StringValue = "";
            SubController = new SetupSubLoginController (Controller);
            Content.ContentView = SubController.View;
        }

        void ShowRepoSelectPage()
        {
            Header.StringValue = Properties_Resources.Which;
            Description.StringValue = "";
            SubController = new SetupSubRepoSelectController (Controller);
            Content.ContentView = SubController.View;
        }
    }
}

