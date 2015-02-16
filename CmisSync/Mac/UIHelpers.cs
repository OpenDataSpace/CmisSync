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

﻿namespace CmisSync {
    using System;
    using System.IO;

    using MonoMac.Foundation;

    using CmisSync.Lib.Config;

    public static class UIHelpers {
        public static string GetImagePathname(string name, string type = "png") {
            string filename = name + "." + type;

            string brandFolder = Path.Combine(ConfigManager.CurrentConfig.GetConfigPath(), Program.Controller.BrandConfigFolder);
            string pathname = FindImagePathname(brandFolder, filename);
            if (!string.IsNullOrEmpty(pathname)) {
                return pathname;
            }

            pathname = Path.Combine(NSBundle.MainBundle.ResourcePath, "Pixmaps", filename);
            if (File.Exists(pathname)) {
                return pathname;
            }

            return Path.Combine(NSBundle.MainBundle.ResourcePath, filename);
        }

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