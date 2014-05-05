using System;
using System.Diagnostics;
using System.IO;

namespace CmisSync.Notifications
{
    public static class NotificationUtils
    {
        /// <summary>
        /// Creates a Notification by calling the external process "notify-send"
        /// </summary>
        /// <param name='title'>
        /// Title.
        /// </param>
        /// <param name='content'>
        /// Content.
        /// </param>
        /// <param name='iconPath'>
        /// Icon path.
        /// </param>
        public static void NotifyAsync(string title, string content = null, string iconPath = null)
        {
            string IconPath = Path.Combine("/","usr","share","icons","hicolor", "32x32", "apps", "app-cmissync.png");
            if(!String.IsNullOrEmpty(iconPath))
            {
                IconPath = iconPath;
            }
            Process process = new Process();
            process.StartInfo.FileName  = "notify-send";
            if(String.IsNullOrEmpty(content))
            {
                process.StartInfo.Arguments = String.Format("-i \"{0}\" \"{1}\"", IconPath, title);
            }
            else
            {
                process.StartInfo.Arguments = String.Format("-i \"{0}\" \"{1}\" \"{2}\"", IconPath, title, content);
            }
            process.Start();
        }
    }
}

