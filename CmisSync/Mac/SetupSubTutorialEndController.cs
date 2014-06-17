//-----------------------------------------------------------------------
// <copyright file="SetupSubTutorialEndController.cs" company="GRAU DATA AG">
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
    public partial class SetupSubTutorialEndController : MonoMac.AppKit.NSViewController
    {

        #region Constructors

        // Called when created from unmanaged code
        public SetupSubTutorialEndController (IntPtr handle) : base (handle)
        {
            Initialize ();
        }
        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public SetupSubTutorialEndController (NSCoder coder) : base (coder)
        {
            Initialize ();
        }
        // Call to load from the XIB/NIB file
        public SetupSubTutorialEndController (SetupController controller) : base ("SetupSubTutorialEnd", NSBundle.MainBundle)
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

			this.StartCheck.Title = String.Format(Properties_Resources.Startup, Properties_Resources.ApplicationName);
            this.FinishButton.Title = Properties_Resources.Finish;
//            this.FinishButton.KeyEquivalent = "\r";

            NSImage image = new NSImage (Path.Combine (NSBundle.MainBundle.ResourcePath, "Pixmaps", "tutorial-slide-" + Controller.TutorialCurrentPage + ".png")) {
                Size = new SizeF (350, 200)
            };
            TutorialView.Image = image;

            switch (Controller.TutorialCurrentPage) {
            case 4:
                TutorialText.StringValue = Properties_Resources.YouCan;
                OnStart (this);
                break;
            }
        }

        partial void OnStart (MonoMac.Foundation.NSObject sender)
        {
            Controller.StartupItemChanged(StartCheck.State == NSCellStateValue.On);
        }

        partial void OnFinish (MonoMac.Foundation.NSObject sender)
        {
            Controller.TutorialPageCompleted();
        }

        //strongly typed view accessor
        public new SetupSubTutorialEnd View {
            get {
                return (SetupSubTutorialEnd)base.View;
            }
        }
    }
}

