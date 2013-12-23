using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

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
        public SetupSubLoginController () : base ("SetupSubLogin", NSBundle.MainBundle)
        {
            Initialize ();
        }
        // Shared initialization code
        void Initialize ()
        {
        }

        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();

            this.AddressLabel.StringValue = Properties_Resources.EnterWebAddress;
            this.UserLabel.StringValue = Properties_Resources.User;
            this.PasswordLabel.StringValue = Properties_Resources.Password;
        }

        #endregion

        //strongly typed view accessor
        public new SetupSubLogin View {
            get {
                return (SetupSubLogin)base.View;
            }
        }
    }
}

