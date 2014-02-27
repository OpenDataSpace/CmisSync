using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace CmisSync
{
    public partial class GeneralSettingsController : MonoMac.AppKit.NSWindowController
    {
        #region Constructors

        // Called when created from unmanaged code
        public GeneralSettingsController(IntPtr handle) : base(handle)
        {
            Initialize();
        }
        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public GeneralSettingsController(NSCoder coder) : base(coder)
        {
            Initialize();
        }
        // Call to load from the XIB/NIB file
        public GeneralSettingsController() : base("GeneralSettings")
        {
            Initialize();
        }
        // Shared initialization code
        void Initialize()
        {
        }

        #endregion

        //strongly typed window accessor
        public new GeneralSettings Window
        {
            get
            {
                return (GeneralSettings)base.Window;
            }
        }

        private NSViewController ProxySubController_ = null;
        private NSViewController ProxySubController {
            get { return ProxySubController_; }
            set {
                if (ProxySubController_ != null) {
                    ProxySubController_.Dispose ();
                    ProxySubController_ = null;
                }
                ProxySubController_ = value;
            }
        }
    }
}

