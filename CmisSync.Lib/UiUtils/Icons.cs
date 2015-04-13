//-----------------------------------------------------------------------
// <copyright file="Icons.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.UiUtils {
    using System;
    using System.Reflection;

    /// <summary>
    /// Icons available for UI.
    /// </summary>
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

    /// <summary>
    /// Icon convenience extenders.
    /// </summary>
    public static class IconExtenders {
        public static string GetName(this Icons icon) {
            var attr = icon.GetAttribute();
            if (attr != null) {
                return attr.Name;
            } else {
                return null;
            }
        }

        public static string GetNameWithTypeExtension(this Icons icon) {
            var attr = icon.GetAttribute();
            if (attr != null) {
                return string.Format("{0}.{1}", attr.Name, attr.FileType);
            } else {
                return null;
            }
        }

        public static string GetNameWithSizeAndTypeExtension(this Icons icon) {
            var attr = icon.GetAttribute() as LinuxIcon;
            if (attr != null) {
                return string.Format("{0}-{1}.{2}", attr.Name, attr.X.ToString(), attr.FileType);
            } else {
                return icon.GetNameWithTypeExtension();
            }
        }

        /// <summary>
        /// Gets the icon attribute of the given icon based on the actual OS.
        /// </summary>
        /// <returns>The icon attribute.</returns>
        /// <param name="icon">Given Icon.</param>
        internal static IconAttribute GetAttribute(this Icons icon) {
            Type type = icon.GetType();
            string name = Enum.GetName(type, icon);
            if (name != null) {
                FieldInfo field = type.GetField(name);
                if (field != null) {
                    switch (Environment.OSVersion.Platform) {
                    case PlatformID.MacOSX:
                        return Attribute.GetCustomAttribute(field, typeof(MacOSIcon)) as MacOSIcon;
                    case PlatformID.Unix:
                        return Attribute.GetCustomAttribute(field, typeof(LinuxIcon)) as LinuxIcon;
                    default:
                        return Attribute.GetCustomAttribute(field, typeof(WindowsIcon)) as WindowsIcon;
                    }
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Linux icon.
    /// </summary>
    internal class LinuxIcon : IconAttribute {
        public LinuxIcon(string name, int x = 16, int y = -1, string type = "png") : base(name, type) {
            this.X = x;
            this.Y = y < 0 ? x : y;
        }

        /// <summary>
        /// Gets the icon width.
        /// </summary>
        /// <value>The width</value>
        public int X { get; private set; }

        /// <summary>
        /// Gets the icon height.
        /// </summary>
        /// <value>The height</value>
        public int Y { get; private set; }
    }

    /// <summary>
    /// Windows icon.
    /// </summary>
    internal class WindowsIcon : IconAttribute {
        public WindowsIcon(string name, string type = "png") : base(name, type) {
        }
    }

    /// <summary>
    /// Mac OS icon.
    /// </summary>
    internal class MacOSIcon : IconAttribute {
        public MacOSIcon(string name, string type = "png") : base(name, type) {
        }
    }

    /// <summary>
    /// Icon attribute.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field)]
    internal class IconAttribute : System.Attribute {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.UiUtils.IconAttribute"/> class.
        /// </summary>
        /// <param name="name">Name of the icon.</param>
        /// <param name="type">Type of the icon.</param>
        public IconAttribute(string name, string type) {
            this.Name = name;
            this.FileType = type;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the type of the file.
        /// </summary>
        /// <value>The type of the file.</value>
        public string FileType { get; private set; }
    }
}