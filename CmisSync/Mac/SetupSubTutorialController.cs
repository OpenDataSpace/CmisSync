using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace CmisSync
{
    public partial class SetupSubTutorialController : MonoMac.AppKit.NSViewController
    {

        #region Constructors

        // Called when created from unmanaged code
        public SetupSubTutorialController (IntPtr handle) : base (handle)
        {
            Initialize ();
        }
        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public SetupSubTutorialController (NSCoder coder) : base (coder)
        {
            Initialize ();
        }
        // Call to load from the XIB/NIB file
        public SetupSubTutorialController (SetupController controller) : base ("SetupSubTutorial", NSBundle.MainBundle)
        {
            this.Controller = controller;
            Initialize ();
        }
        // Shared initialization code
        void Initialize ()
        {
        }

        #endregion

        SetupController Controller;

        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();

            this.SkipButton.Title = Properties_Resources.SkipTutorial;
            this.ContinueButton.Title = Properties_Resources.Continue;

            NSImage image = new NSImage (Path.Combine (NSBundle.MainBundle.ResourcePath, "Pixmaps", "tutorial-slide-" + Controller.TutorialCurrentPage + ".png")) {
                Size = new SizeF (350, 200)
            };
            TutorialView.Image = image;

            switch (Controller.TutorialCurrentPage) {
            case 1:
                TutorialText.StringValue = Properties_Resources.CmisSyncCreates;
                break;
            case 2:
                TutorialText.StringValue = Properties_Resources.DocumentsAre;
                break;
            case 3:
                TutorialText.StringValue = Properties_Resources.StatusIconShows;
                break;
            case 4:
                TutorialText.StringValue = Properties_Resources.YouCan;
//                    StartupCheckButton = new NSButton() {
//                        Frame = new RectangleF(190, Frame.Height - 400, 300, 18),
//                        Title = Properties_Resources.Startup,
//                        State = NSCellStateValue.On
//                    };
//                    StartupCheckButton.SetButtonType(NSButtonType.Switch);
//                    FinishButton = new NSButton() {
//                        Title = Properties_Resources.Finish
//                    };
//                    StartupCheckButton.Activated += delegate
//                    {
//                        Controller.StartupItemChanged(StartupCheckButton.State == NSCellStateValue.On);
//                    };
//                    ContentView.AddSubview(StartupCheckButton);
                break;
            }
        }

        partial void OnSkip (MonoMac.Foundation.NSObject sender)
        {
            Controller.TutorialSkipped();
        }

        partial void OnContinue (MonoMac.Foundation.NSObject sender)
        {
            Controller.TutorialPageCompleted();
        }

        //strongly typed view accessor
        public new SetupSubTutorial View {
            get {
                return (SetupSubTutorial)base.View;
            }
        }
    }
}

