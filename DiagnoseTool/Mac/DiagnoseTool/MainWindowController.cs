//-----------------------------------------------------------------------
// <copyright file="MainWindowController.cs" company="GRAU DATA AG">
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
﻿
namespace DiagnoseTool {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using MonoMac.AppKit;
    using MonoMac.Foundation;

    public partial class MainWindowController : MonoMac.AppKit.NSWindowController {
        #region Constructors

        // Called when created from unmanaged code
        public MainWindowController(IntPtr handle) : base(handle) {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public MainWindowController(NSCoder coder) : base(coder) {
            Initialize();
        }

        // Call to load from the XIB/NIB file
        public MainWindowController() : base("MainWindow") {
            Initialize();
        }

        // Shared initialization code
        void Initialize() {
        }

        #endregion

        //strongly typed window accessor
        public new MainWindow Window {
            get {
                return (MainWindow)base.Window;
            }
        }
    }
}