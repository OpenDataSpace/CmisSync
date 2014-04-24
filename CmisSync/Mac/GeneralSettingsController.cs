//-----------------------------------------------------------------------
// <copyright file="GeneralSettingsController.cs" company="GRAU DATA AG">
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
        public SettingController Controller = new SettingController();
        //strongly typed window accessor
        public new GeneralSettings Window
        {
            get
            {
                return (GeneralSettings)base.Window;
            }
        }

        public enum Type {
            DEFAULT = 0,
            PROXY,
            BANDWIDTH,
            DEVICEMANAGEMENT
        }
    }
}

