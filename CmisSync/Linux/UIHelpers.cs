//-----------------------------------------------------------------------
// <copyright file="UIHelpers.cs" company="GRAU DATA AG">
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
//   CmisSync, a collaboration and sharing tool.
//   Copyright (C) 2010  Hylke Bons <hylkebons@gmail.com>
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace CmisSync {
    using System;
    using System.IO;

    using CmisSync.Lib.Config;

    using Gtk;

    public static class UIHelpers {
        // Looks up an icon from the system's theme
        [CLSCompliant(false)]
        public static Gdk.Pixbuf GetIcon(string name, int size) {
            IconTheme icon_theme = new IconTheme();
            icon_theme.AppendSearchPath(Path.Combine(UI.AssetsPath, "icons"));

            try {
                return icon_theme.LoadIcon(name, size, IconLookupFlags.GenericFallback);
            } catch {
                try {
                    return icon_theme.LoadIcon("gtk-image-missing", size, IconLookupFlags.GenericFallback);
                } catch {
                    return null;
                }
            }
        }

        [CLSCompliant(false)]
        public static Image GetImage(string name) {
            return new Image(GetImagePath(name));
        }

        [CLSCompliant(false)]
        public static string GetImagePath(string name) {
            string brandFolder = Path.Combine(ConfigManager.CurrentConfig.GetConfigPath(), Program.Controller.BrandConfigFolder);
            string image_path = FindImagePathname(brandFolder, name);
            if (!string.IsNullOrEmpty(image_path)) {
                return image_path;
            }

            image_path = Path.Combine(UI.AssetsPath, "pixmaps", name);
            return image_path;
        }

        // Converts a Gdk RGB color to a hex value.
        // Example: from "rgb:0,0,0" to "#000000"
        [CLSCompliant(false)]
        public static string ToHex(this Gdk.Color color) {
            return string.Format(
                "#{0:X2}{1:X2}{2:X2}",
                (int)Math.Truncate(color.Red   / 256.00),
                (int)Math.Truncate(color.Green / 256.00),
                (int)Math.Truncate(color.Blue  / 256.00));
        }

        [CLSCompliant(false)]
        private static string FindImagePathname(string folder, string filename) {
            ClientBrand brand = new ClientBrand();
            foreach (string path in brand.PathList) {
                if (Path.GetFileName(path) == filename) {
                    string pathname = Path.Combine(folder, path.Substring(1));
                    if (File.Exists(pathname)) {
                        return pathname;
                    } else {
                        return null;
                    }
                }
            }

            return null;
        }
    }
}