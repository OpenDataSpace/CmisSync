
namespace CmisSync.Lib.Cmis.UiUtils {
    using System;

    public enum Icons {
        [LinuxIcon("dataspacesync-app")]
        [WindowsIcon("dataspacesync", "ico")]
        [MacOSIcon("dataspacesync", "icns")]
        APPLICATION_ICON
    }

    public class LinuxIcon: IconAttribute {
        public LinuxIcon(string name, int size = 16, string type = "png") : base(name, type) {
            this.Size = size;
        }

        public int Size { get; private set; }
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