
namespace DiagnoseTool {
    using System;
    using System.Drawing;

    using MonoMac.AppKit;
    using MonoMac.Foundation;
    using MonoMac.ObjCRuntime;

    class MainClass {
        static void Main(string[] args) {
            NSApplication.Init();
            NSApplication.Main(args);
        }
    }
}