//-----------------------------------------------------------------------
// <copyright file="SetupSubLoginController.cs" company="GRAU DATA AG">
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
using System.Globalization;
using System.Threading;
using MonoMac.Foundation;
using MonoMac.AppKit;

using CmisSync.Lib.Config;
using CmisSync.Lib.Cmis;
using CmisSync.Lib.Cmis.UiUtils;

namespace CmisSync
{
    public partial class SetupSubLoginController : MonoMac.AppKit.NSViewController
    {

        #region Constructors

        // Called when created from unmanaged code
        public SetupSubLoginController (IntPtr handle) : base (handle)
        {
            Initialize ();
        }
        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public SetupSubLoginController (NSCoder coder) : base (coder)
        {
            Initialize ();
        }
        // Call to load from the XIB/NIB file
        public SetupSubLoginController (SetupController controller) : base ("SetupSubLogin", NSBundle.MainBundle)
        {
            this.Controller = controller;
            Initialize ();
        }
        // Shared initialization code
        void Initialize ()
        {
        }

        #endregion

        protected override void Dispose (bool disposing)
        {
            base.Dispose (disposing);
            Console.WriteLine (this.GetType ().ToString () + " disposed " + disposing.ToString ());
        }

        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();

            this.AddressLabel.StringValue = Properties_Resources.EnterWebAddress;
            this.UserLabel.StringValue = Properties_Resources.User;
            this.PasswordLabel.StringValue = Properties_Resources.Password;

            this.AddressDelegate = new TextFieldDelegate ();
            this.AddressText.Delegate = this.AddressDelegate;

            this.ContinueButton.Title = Properties_Resources.Continue;
            this.CancelButton.Title = Properties_Resources.Cancel;

            this.AddressText.StringValue = (Controller.PreviousAddress == null || String.IsNullOrEmpty (Controller.PreviousAddress.ToString ())) ? "https://" : Controller.PreviousAddress.ToString ();
            this.UserText.StringValue = String.IsNullOrEmpty (Controller.saved_user) ? Environment.UserName : Controller.saved_user;
//            this.PasswordText.StringValue = String.IsNullOrEmpty (Controller.saved_password) ? "" : Controller.saved_password;
            this.PasswordText.StringValue = "";

            InsertEvent ();

            //  Must be called after InsertEvent()
            CheckAddressTextField ();
        }

        void InsertEvent()
        {
            this.AddressDelegate.StringValueChanged += CheckAddressTextField;
            Controller.UpdateSetupContinueButtonEvent += SetContinueButton;
            Controller.UpdateAddProjectButtonEvent += SetContinueButton;
        }

        void RemoveEvent()
        {
            this.AddressDelegate.StringValueChanged -= CheckAddressTextField;
            Controller.UpdateSetupContinueButtonEvent -= SetContinueButton;
            Controller.UpdateAddProjectButtonEvent -= SetContinueButton;
        }

        void SetContinueButton(bool enabled)
        {
            InvokeOnMainThread (delegate
            {
                ContinueButton.Enabled = enabled;
//                ContinueButton.KeyEquivalent = "\r";
            });
        }

        void CheckAddressTextField()
        {
            InvokeOnMainThread (delegate
            {
                string error = Controller.CheckAddPage (AddressText.StringValue);
                if (String.IsNullOrEmpty (error))
                    AddressHelp.StringValue = "";
                else
                    AddressHelp.StringValue = Properties_Resources.ResourceManager.GetString (error, CultureInfo.CurrentCulture);
            });
        }

        SetupController Controller;
        TextFieldDelegate AddressDelegate;

        partial void OnCancel (MonoMac.Foundation.NSObject sender)
        {
            RemoveEvent();
            Controller.PageCancelled();
        }

        partial void OnContinue (MonoMac.Foundation.NSObject sender)
        {
            ServerCredentials credentials = new ServerCredentials() {
                UserName = UserText.StringValue,
                Password = PasswordText.StringValue,
                Address = new Uri(AddressText.StringValue),
                Binding = (Controller.saved_binding == null) ? ServerCredentials.BindingBrowser : Controller.saved_binding
            };
            WarnText.StringValue = String.Empty;
            AddressText.Enabled = false;
            UserText.Enabled = false;
            PasswordText.Enabled = false;
            ContinueButton.Enabled = false;
            CancelButton.Enabled = false;
            //  monomac bug: animation GUI effect will cause GUI to hang, when backend thread is busy
//            LoginProgress.StartAnimation(this);
            Thread check = new Thread(() => {
                Tuple<CmisServer, Exception> fuzzyResult = CmisUtils.GetRepositoriesFuzzy(credentials);
                CmisServer cmisServer = fuzzyResult.Item1;
                if (cmisServer != null)
                {
                    Controller.repositories = cmisServer.Repositories;
                }
                else
                {
                    Controller.repositories = null;
                }
                InvokeOnMainThread(delegate {
                    if (Controller.repositories == null)
                    {
                        WarnText.StringValue = Controller.GetConnectionsProblemWarning(fuzzyResult.Item1, fuzzyResult.Item2);
                        AddressText.Enabled = true;
                        UserText.Enabled = true;
                        PasswordText.Enabled = true;
                        ContinueButton.Enabled = true;
                        CancelButton.Enabled = true;
                    }
                    else
                    {
                        RemoveEvent();
                        Controller.Add1PageCompleted(cmisServer.Url, cmisServer.Binding, credentials.UserName, credentials.Password.ToString());
                    }
                    LoginProgress.StopAnimation(this);
                });
            });
            check.Start();
        }


        //strongly typed view accessor
        public new SetupSubLogin View {
            get {
                return (SetupSubLogin)base.View;
            }
        }
    }
}

