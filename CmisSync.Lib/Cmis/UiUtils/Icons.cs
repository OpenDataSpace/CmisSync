
namespace CmisSync.Lib.Cmis.UiUtils {
    using System;

    public enum Icons {
        [LinuxIcon("dataspacesync-app")]
        [WindowsIcon("dataspacesync", "ico")]
        [MacOSIcon("dataspacesync", "icns")]
        APPLICATION_ICON
    }

    public class LinuxIcon: IconAttribute {
        public LinuxIcon(string name, int x = 16, int y = -1, string type = "png") : base(name, type) {
            this.X = x;
            this.Y = y < 0 ? x : y;
        }

        public int X { get; private set; }
        public int Y { get; private set; }
    }

    public class WindowsIcon : IconAttribute {
        public WindowsIcon(string name, string type = "png") : base(name, type) {
        }
    }

    public class MacOSIcon : IconAttribute {
        public MacOSIcon(string name, string type = "png") : base(name, type) {
        }
    }

    public class IconAttribute : System.Attribute {
        public IconAttribute(string name, string type) {
            this.Name = name;
            this.FileType = type;
        }

        public string Name { get; private set; }
        public string FileType {get; private set; }
    }
}