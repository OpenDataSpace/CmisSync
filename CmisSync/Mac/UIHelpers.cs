namespace CmisSync
{
    using System;
    using System.IO;

    using MonoMac.Foundation;

    using CmisSync.Lib.Config;

    public static class UIHelpers
    {
        public static string GetImagePathname (string name, string type = "png")
        {
            string filename = name + "." + type;

            string brandFolder = Path.Combine (ConfigManager.CurrentConfig.GetConfigPath (), Program.Controller.BrandConfigFolder);
            string pathname = FindImagePathname (brandFolder, filename);
            if (!string.IsNullOrEmpty (pathname)) {
                return pathname;
            }

            pathname = Path.Combine (NSBundle.MainBundle.ResourcePath, "Pixmaps", filename);
            if (File.Exists (pathname)) {
                return pathname;
            }

            return Path.Combine (NSBundle.MainBundle.ResourcePath, filename);
        }

        private static string FindImagePathname (string folder, string filename)
        {
            ClientBrand brand = new ClientBrand ();
            foreach (string path in brand.GetPathList()) {
                if (Path.GetFileName (path) == filename) {
                    string pathname = Path.Combine (folder, path.Substring(1));
                    if (File.Exists (pathname)) {
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

