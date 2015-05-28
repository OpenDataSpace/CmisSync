//-----------------------------------------------------------------------
// <copyright file="AppDelegate.cs" company="GRAU DATA AG">
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
    using System.Drawing;

    using MonoMac.AppKit;
    using MonoMac.Foundation;
    using MonoMac.ObjCRuntime;

    public partial class AppDelegate : NSApplicationDelegate {
        MainWindowController mainWindowController;

        public AppDelegate() {
        }

        public override void DidFinishLaunching(NSNotification notification) {
            mainWindowController = new MainWindowController();
            mainWindowController.Window.MakeKeyAndOrderFront(this);
        }

        public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) {
            return true;
        }
    }
}