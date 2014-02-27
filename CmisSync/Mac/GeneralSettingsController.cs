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
            Controller.HideWindowEvent += delegate
            {
                using (var a = new NSAutoreleasePool ())
                {
                    InvokeOnMainThread (delegate {
                        Window.PerformClose (this);
                    });
                }
            };

            Controller.ShowWindowEvent += delegate
            {
                using (var a = new NSAutoreleasePool ())
                {
                    InvokeOnMainThread (delegate {
                        Window.OrderFrontRegardless ();
                    });
                }
            };

        }

        #endregion
        private SettingController Controller = new SettingController();
        //strongly typed window accessor
        public new GeneralSettings Window
        {
            get
            {
                return (GeneralSettings)base.Window;
            }
        }

        public void OnCancel()
        {
            using (var a = new NSAutoreleasePool())
            {
                InvokeOnMainThread(delegate
                {
                    Window.PerformClose(this.Window);
                });
            }
        }
    }
}

