
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