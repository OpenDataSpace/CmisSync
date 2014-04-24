using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace CmisSync
{
    public partial class ProxySettingsController : MonoMac.AppKit.NSViewController
    {
        #region Constructors

        // Called when created from unmanaged code
        public ProxySettingsController(IntPtr handle) : base(handle)
        {
            Initialize();
        }
        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public ProxySettingsController(NSCoder coder) : base(coder)
        {
            Initialize();
        }
        // Call to load from the XIB/NIB file
        public ProxySettingsController() : base("ProxySettings", NSBundle.MainBundle)
        {
            Initialize();
        }
        // Shared initialization code
        void Initialize()
        {
        }

        #endregion

        //strongly typed view accessor
        public new ProxySettings View
        {
            get
            {
                return (ProxySettings)base.View;
            }
        }
    }
}

