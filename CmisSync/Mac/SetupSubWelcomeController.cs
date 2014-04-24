//-----------------------------------------------------------------------
// <copyright file="SetupSubWelcomeController.cs" company="GRAU DATA AG">
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
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace CmisSync
{
    public partial class SetupSubWelcomeController : MonoMac.AppKit.NSViewController
    {

        #region Constructors

        // Called when created from unmanaged code
        public SetupSubWelcomeController (IntPtr handle) : base (handle)
        {
            Initialize ();
        }
        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public SetupSubWelcomeController (NSCoder coder) : base (coder)
        {
            Initialize ();
        }
        // Call to load from the XIB/NIB file
        public SetupSubWelcomeController (SetupController controller) : base ("SetupSubWelcome", NSBundle.MainBundle)
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

			this.WelcomeText.StringValue = String.Format(Properties_Resources.Intro, Properties_Resources.ApplicationName);

            this.CancelButton.Title = Properties_Resources.Cancel;
            this.ContinueButton.Title = Properties_Resources.Continue;
//            this.ContinueButton.KeyEquivalent = "\r";
        }

        partial void OnCancel (MonoMac.Foundation.NSObject sender)
        {
            Controller.SetupPageCancelled();
        }

        partial void OnContinue (MonoMac.Foundation.NSObject sender)
        {
            Controller.SetupPageCompleted();
        }

        //strongly typed view accessor
        public new SetupSubWelcome View {
            get {
                return (SetupSubWelcome)base.View;
            }
        }
    }
}

