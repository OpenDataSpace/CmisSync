using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Threading;
using MonoMac.Foundation;
using MonoMac.AppKit;

using CmisSync.Lib.Credentials;
using CmisSync.Lib.Cmis;

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

        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();

            this.AddressLabel.StringValue = Properties_Resources.EnterWebAddress;
            this.UserLabel.StringValue = Properties_Resources.User;
            this.PasswordLabel.StringValue = Properties_Resources.Password;

            this.AddressDelegate = new TextFieldDelegate ();
            this.AddressDelegate.StringValueChanged += CheckAddressTextField;
            this.AddressText.Delegate = this.AddressDelegate;

            this.ContinueButton.Title = Properties_Resources.Continue;
            this.CancelButton.Title = Properties_Resources.Cancel;

            this.AddressText.StringValue = (Controller.PreviousAddress == null || String.IsNullOrEmpty (Controller.PreviousAddress.ToString ())) ? "https://" : Controller.PreviousAddress.ToString ();
            this.UserText.StringValue = String.IsNullOrEmpty (Controller.saved_user) ? Environment.UserName : Controller.saved_user;
            this.PasswordText.StringValue = String.IsNullOrEmpty (Controller.saved_password) ? "" : Controller.saved_password;
        }

        void CheckAddressTextField()
        {
            string error = Controller.CheckAddPage (AddressText.StringValue);
            if (String.IsNullOrEmpty (error))
                AddressHelp.StringValue = "";
            else
                AddressHelp.StringValue = Properties_Resources.ResourceManager.GetString (error, CultureInfo.CurrentCulture);
        }

        ~SetupSubLoginController()
        {
            this.AddressDelegate.StringValueChanged -= CheckAddressTextField;
        }

        SetupController Controller;
        TextFieldDelegate AddressDelegate;

        partial void OnCancel (MonoMac.Foundation.NSObject sender)
        {
            Controller.PageCancelled();
        }

        partial void OnContinue (MonoMac.Foundation.NSObject sender)
        {
            ServerCredentials credentials = new ServerCredentials() {
                UserName = UserText.StringValue,
                Password = PasswordText.StringValue,
                Address = new Uri(AddressText.StringValue)
            };
            ContinueButton.Enabled = false;
            CancelButton.Enabled = false;

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
                        AddressHelp.StringValue = Controller.getConnectionsProblemWarning(fuzzyResult.Item1, fuzzyResult.Item2);
                        ContinueButton.Enabled = true;
                        CancelButton.Enabled = true;
                    }
                    else
                    {
                        Controller.Add1PageCompleted(cmisServer.Url, credentials.UserName, credentials.Password.ToString());
                    }
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

