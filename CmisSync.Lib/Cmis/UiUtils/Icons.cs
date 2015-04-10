
namespace CmisSync.Lib.Cmis.UiUtils {
    using System;

    public enum Icons {
        [LinuxIcon("dataspacesync-app")]
        [WindowsIcon("app", "ico")]
        [MacOSIcon("cmissync-app", "icns")]
        ApplicationIcon,

        [LinuxIcon("dataspacesync-folder")]
        [WindowsIcon("cmissync-folder", "ico")]
        [MacOSIcon("cmissync-folder", "icns")]
        DefaultTargetFolderIcon,

        [LinuxIcon("dataspacesync-folder")]
        [WindowsIcon("folder")]
        [MacOSIcon("cmissync-folder", "icns")]
        FolderIcon,

        [LinuxIcon("dataspacesync-process-syncing-i")]
        [WindowsIcon("process-syncing-i")]
        [MacOSIcon("process-syncing-i")]
        SyncInProgressIcon1,

        [LinuxIcon("dataspacesync-process-syncing-ii")]
        [WindowsIcon("process-syncing-ii")]
        [MacOSIcon("process-syncing-ii")]
        SyncInProgressIcon2,

        [LinuxIcon("dataspacesync-process-syncing-iii")]
        [WindowsIcon("process-syncing-iii")]
        [MacOSIcon("process-syncing-iii")]
        SyncInProgressIcon3,

        [LinuxIcon("dataspacesync-process-syncing-iiii")]
        [WindowsIcon("process-syncing-iiii")]
        [MacOSIcon("process-syncing-iiii")]
        SyncInProgressIcon4,

        [LinuxIcon("dataspacesync-process-syncing-iiiii")]
        [WindowsIcon("process-syncing-iiiii")]
        [MacOSIcon("process-syncing-iiiii")]
        SyncInProgressIcon5,

        [LinuxIcon("dataspacesync-process-syncing-i")]
        [WindowsIcon("process-syncing-i")]
        [MacOSIcon("process-syncing-i")]
        SyncPausedIcon,

        [MacOSIcon("process-syncing-i-active")]
        SyncPausedHighlightedIcon,

        [LinuxIcon("dataspacesync-process-syncing-i")]
        [WindowsIcon("process-syncing-i")]
        [MacOSIcon("process-syncing-i")]
        SyncDisconnectedIcon,

        [MacOSIcon("process-syncing-i-active")]
        SyncDisconnectedHighlightedIcon,

        [LinuxIcon("dataspacesync-process-syncing-i")]
        [WindowsIcon("process-syncing-i")]
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
        [WindowsIcon("process-syncing-error")]
        [MacOSIcon("process-syncing-error")]
        ErrorOnSyncIcon,

        [MacOSIcon("process-syncing-error-active")]
        ErrorOnSyncHighlightedIcon,

        [LinuxIcon("about")]
        [WindowsIcon("about")]
        [MacOSIcon("about")]
        AboutImage,

        [LinuxIcon("side-splash")]
        [WindowsIcon("side-splash")]
        [MacOSIcon("side-splash")]
        SideImage,

        [LinuxIcon("dataspacesync-process-syncing-error", 24)]
        [WindowsIcon("process-syncing-error")]
        [MacOSIcon("error", "icns")]
        ErrorIcon,

        [LinuxIcon("tutorial-slide-1")]
        [WindowsIcon("tutorial-slide-1")]
        [MacOSIcon("tutorial-slide-1")]
        TutorialSlide1,

        [LinuxIcon("tutorial-slide-2")]
        [WindowsIcon("tutorial-slide-2")]
        [MacOSIcon("tutorial-slide-2")]
        TutorialSlide2,

        [LinuxIcon("tutorial-slide-3")]
        [WindowsIcon("tutorial-slide-3")]
        [MacOSIcon("tutorial-slide-3")]
        TutorialSlide3,

        [LinuxIcon("tutorial-slide-4")]
        [WindowsIcon("tutorial-slide-4")]
        [MacOSIcon("tutorial-slide-4")]
        TutorialSlide4,

        [LinuxIcon("dataspacesync-start", 12)]
        [WindowsIcon("media_playback_start")]
        [MacOSIcon("media_playback_start")]
        ResumeIcon,

        [LinuxIcon("dataspacesync-pause", 12)]
        [WindowsIcon("media_playback_pause")]
        [MacOSIcon("media_playback_pause")]
        PauseIcon,

        [LinuxIcon("dataspacesync-uploading")]
        [WindowsIcon("Uploading")]
        [MacOSIcon("Uploading")]
        UploadNewObjectIcon,

        [LinuxIcon("dataspacesync-updating")]
        [WindowsIcon("Updating")]
        [MacOSIcon("Updating")]
        UploadAndUpdateExistingObjectIcon,

        [LinuxIcon("dataspacesync-downloading")]
        [WindowsIcon("Downloading")]
        [MacOSIcon("Downloading")]
        DownloadNewObjectIcon,

        [LinuxIcon("dataspacesync-updating")]
        [WindowsIcon("Updating")]
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