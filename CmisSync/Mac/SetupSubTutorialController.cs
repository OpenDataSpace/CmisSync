//-----------------------------------------------------------------------
// <copyright file="SetupSubTutorialController.cs" company="GRAU DATA AG">
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

            this.ContinueButton.Title = Properties_Resources.Continue;
//            this.ContinueButton.KeyEquivalent = "\r";

            NSImage image = new NSImage (UIHelpers.GetImagePathname ("tutorial-slide-" + Controller.TutorialCurrentPage)) {
                Size = new SizeF (350, 200)
            };
            TutorialView.Image = image;

            switch (Controller.TutorialCurrentPage) {
            case 2:
                TutorialText.StringValue = Properties_Resources.DocumentsAre;
                break;
            case 3:
				TutorialText.StringValue = String.Format(Properties_Resources.StatusIconShows, Properties_Resources.ApplicationName);
                break;
            }
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

