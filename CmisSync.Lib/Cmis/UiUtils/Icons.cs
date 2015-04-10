
namespace CmisSync.Lib.Cmis.UiUtils {
    using System;

    public enum Icons {
        [LinuxIcon("dataspacesync-app")]
        [WindowsIcon("dataspacesync", "ico")]
        [MacOSIcon("cmissync-app", "icns")]
        ApplicationIcon,

        [LinuxIcon("dataspacesync-folder")]
        [WindowsIcon("")]
        [MacOSIcon("cmissync-folder", "icns")]
        DefaultTargetFolderIcon,

        [LinuxIcon("dataspacesync-folder")]
        [WindowsIcon("")]
        [MacOSIcon("cmissync-folder", "icns")]
        FolderIcon,

        [LinuxIcon("dataspacesync-process-syncing-i")]
        [WindowsIcon("")]
        [MacOSIcon("process-syncing-i")]
        SyncInProgressIcon1,

        [LinuxIcon("dataspacesync-process-syncing-ii")]
        [WindowsIcon("")]
        [MacOSIcon("process-syncing-ii")]
        SyncInProgressIcon2,

        [LinuxIcon("dataspacesync-process-syncing-iii")]
        [WindowsIcon("")]
        [MacOSIcon("process-syncing-iii")]
        SyncInProgressIcon3,

        [LinuxIcon("dataspacesync-process-syncing-iiii")]
        [WindowsIcon("")]
        [MacOSIcon("process-syncing-iiii")]
        SyncInProgressIcon4,

        [LinuxIcon("dataspacesync-process-syncing-iiiii")]
        [WindowsIcon("")]
        [MacOSIcon("process-syncing-iiiii")]
        SyncInProgressIcon5,

        [LinuxIcon("dataspacesync-process-syncing-i")]
        [WindowsIcon("")]
        [MacOSIcon("process-syncing-i")]
        SyncPausedIcon,

        [MacOSIcon("process-syncing-i-active")]
        SyncPausedHighlightedIcon,

        [LinuxIcon("dataspacesync-process-syncing-i")]
        [WindowsIcon("")]
        [MacOSIcon("process-syncing-i")]
        SyncDisconnectedIcon,

        [MacOSIcon("process-syncing-i-active")]
        SyncDisconnectedHighlightedIcon,

        [LinuxIcon("dataspacesync-process-syncing-i")]
        [WindowsIcon("")]
        [MacOSIcon("process-syncing-i")]
        SyncDisabledIcon,

        [MacOSIcon("process-syncing-i-active")]
        SyncDisabledHighlightedIcon,

        [MacOSIcon("process-syncing-i-active")]
        SyncInProgressHighlightedIcon1,

        [MacOSIcon("process-syncing-ii-active")]
        SyncInProgressHighlightedIcon2,

        [MacOSIcon("process-syncing-iii-active")]
        SyncInProgressHighlightedIcon3,

        [MacOSIcon("process-syncing-iiii-active")]
        SyncInProgressHighlightedIcon4,

        [MacOSIcon("process-syncing-iiiii-active")]
        SyncInProgressHighlightedIcon5,

        [LinuxIcon("dataspacesync-process-syncing-error", 24)]
        [WindowsIcon("")]
        [MacOSIcon("process-syncing-error")]
        ErrorOnSyncIcon,

        [MacOSIcon("process-syncing-error-active")]
        ErrorOnSyncHighlightedIcon,

        [LinuxIcon("about")]
        [WindowsIcon("")]
        [MacOSIcon("about")]
        AboutImage,

        [LinuxIcon("side-splash")]
        [WindowsIcon("")]
        [MacOSIcon("side-splash")]
        SideImage,

        [LinuxIcon("dataspacesync-process-syncing-error", 24)]
        [WindowsIcon("")]
        [MacOSIcon("error", "icns")]
        ErrorIcon,

        [LinuxIcon("tutorial-slide-1")]
        [WindowsIcon("")]
        [MacOSIcon("tutorial-slide-1")]
        TutorialSlide1,

        [LinuxIcon("tutorial-slide-2")]
        [WindowsIcon("")]
        [MacOSIcon("tutorial-slide-2")]
        TutorialSlide2,

        [LinuxIcon("tutorial-slide-3")]
        [WindowsIcon("")]
        [MacOSIcon("tutorial-slide-3")]
        TutorialSlide3,

        [LinuxIcon("tutorial-slide-4")]
        [WindowsIcon("")]
        [MacOSIcon("tutorial-slide-4")]
        TutorialSlide4,

        [LinuxIcon("dataspacesync-start", 12)]
        [WindowsIcon("")]
        [MacOSIcon("media_playback_start")]
        ResumeIcon,

        [LinuxIcon("dataspacesync-pause", 12)]
        [WindowsIcon("")]
        [MacOSIcon("media_playback_pause")]
        PauseIcon,

        [LinuxIcon("dataspacesync-uploading")]
        [WindowsIcon("")]
        [MacOSIcon("Uploading")]
        UploadNewObjectIcon,

        [LinuxIcon("dataspacesync-updating")]
        [WindowsIcon("")]
        [MacOSIcon("Updating")]
        UploadAndUpdateExistingObjectIcon,

        [LinuxIcon("dataspacesync-downloading")]
        [WindowsIcon("")]
        [MacOSIcon("Downloading")]
        DownloadNewObjectIcon,

        [LinuxIcon("dataspacesync-updating")]
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

    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class IconAttribute : System.Attribute {
        public IconAttribute(string name, string type) {
            this.Name = name;
            this.FileType = type;
        }

        public string Name { get; private set; }
        public string FileType {get; private set; }
    }
}