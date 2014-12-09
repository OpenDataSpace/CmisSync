//-----------------------------------------------------------------------
// <copyright file="SetupWizardController.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------
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
                    InvokeOnMainThread (delegate {
                        if (!IsWindowLoaded) {
                            LoadWindow();
                        }
                        switch (type)
                        {
                        case PageType.Setup:
                            ShowWelcomePage();
                            break;
                        case PageType.Tutorial:
                            ShowTutorialPage();
                            break;
                        case PageType.Add1:
                            ShowLoginPage();
                            break;
                        case PageType.Add2:
                            ShowRepoSelectPage();
                            break;
                        case PageType.Customize:
                            ShowCustomizePage();
                            break;
                        case PageType.Finished:
                            ShowFinishedPage();
                            break;
                        }
                    });
                }
            };
        }

        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();

            this.SideSplashView.Image = new NSImage (UIHelpers.GetImagePathname ("side-splash")) {
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

        private NSViewController SubController_ = null;
        private NSViewController SubController {
            get { return SubController_; }
            set {
                if (SubController_ != null) {
                    SubController_.Dispose ();
                    SubController_ = null;
                }
                SubController_ = value;
            }
        }

        void ShowLoginPage()
        {
            Header.StringValue = Properties_Resources.Where;
            Description.StringValue = String.Empty;
            SubController = new SetupSubLoginController (Controller);
            Content.ContentView = SubController.View;
        }

        void ShowRepoSelectPage()
        {
            Header.StringValue = Properties_Resources.Which;
            Description.StringValue = String.Empty;
            SubController = new SetupSubRepoSelectController (Controller);
            Content.ContentView = SubController.View;
        }

        void ShowCustomizePage()
        {
            Header.StringValue = Properties_Resources.Customize;
            Description.StringValue = String.Empty;
            SubController = new SetupSubCustomizeController (Controller);
            Content.ContentView = SubController.View;
        }

        void ShowFinishedPage()
        {
            Header.StringValue = Properties_Resources.Ready;
            Description.StringValue = String.Empty;
            SubController = new SetupSubFinishedController (Controller);
            Content.ContentView = SubController.View;
        }

        void ShowWelcomePage()
        {
            Header.StringValue = Properties_Resources.Ready;
            Description.StringValue = String.Empty;
            SubController = new SetupSubWelcomeController (Controller);
            Content.ContentView = SubController.View;
        }

        void ShowTutorialPage()
        {
            SubController = new SetupSubTutorialController (Controller);
            switch (Controller.TutorialCurrentPage) {
            case 1:
                Header.StringValue = Properties_Resources.WhatsNext;
                SubController = new SetupSubTutorialBeginController (Controller);
                break;
            case 2:
                Header.StringValue = Properties_Resources.Synchronization;
                SubController = new SetupSubTutorialController (Controller);
                break;
            case 3:
                Header.StringValue = Properties_Resources.StatusIcon;
                SubController = new SetupSubTutorialController (Controller);
                break;
            case 4:
				Header.StringValue = String.Format(Properties_Resources.AddFolders, Properties_Resources.ApplicationName);
                SubController = new SetupSubTutorialEndController (Controller);
                break;
            }
            Description.StringValue = String.Empty;
            Content.ContentView = SubController.View;
        }

    }
}

