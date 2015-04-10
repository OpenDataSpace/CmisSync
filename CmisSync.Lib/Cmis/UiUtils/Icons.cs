
namespace CmisSync.Lib.Cmis.UiUtils {
    using System;

    public enum Icons {
        [LinuxIcon("dataspacesync-app")]
        [WindowsIcon("dataspacesync", "ico")]
        [MacOSIcon("cmissync-app", "icns")]
        ApplicationIcon,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("cmissync-folder", "icns")]
        DefaultTargetFolderIcon,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("cmissync-folder", "icns")]
        FolderIcon,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("process-syncing-i")]
        RemoteFolderIcon,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("process-syncing-i")]
        SyncInProgressIcon1,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("process-syncing-ii")]
        SyncInProgressIcon2,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("process-syncing-iii")]
        SyncInProgressIcon3,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("process-syncing-iiii")]
        SyncInProgressIcon4,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("process-syncing-iiiii")]
        SyncInProgressIcon5,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("process-syncing-i-active")]
        SyncInProgressHighlightedIcon1,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("process-syncing-ii-active")]
        SyncInProgressHighlightedIcon2,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("process-syncing-iii-active")]
        SyncInProgressHighlightedIcon3,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("process-syncing-iiii-active")]
        SyncInProgressHighlightedIcon4,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("process-syncing-iiiii-active")]
        SyncInProgressHighlightedIcon5,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("process-syncing-error")]
        ErrorOnSyncIcon,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("process-syncing-error-active")]
        ErrorOnSyncHighlightedIcon,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("about")]
        AboutImage,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("side-splash")]
        SideImage,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("error", "icns")]
        ErrorIcon,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("tutorial-slide-1")]
        TutorialSlide1,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("tutorial-slide-2")]
        TutorialSlide2,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("tutorial-slide-3")]
        TutorialSlide3,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("tutorial-slide-4")]
        TutorialSlide4,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("media_playback_start")]
        ResumeIcon,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("media_playback_pause")]
        PauseIcon,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("Uploading")]
        UploadNewObjectIcon,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("Updating")]
        UploadAndUpdateExistingObjectIcon,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("Downloading")]
        DownloadNewObjectIcon,

        [LinuxIcon("")]
        [WindowsIcon("")]
        [MacOSIcon("Updating")]
        DownloadAndUpdateExistingObjectIcon,
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