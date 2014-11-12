//-----------------------------------------------------------------------
// <copyright file="NotificationUtils.cs" company="GRAU DATA AG">
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
using Notifications;

namespace CmisSync.Notifications
{
    using System;
    using System.Diagnostics;
    using System.IO;

    public static class NotificationUtils
    {
        private static readonly string IconName = Path.Combine("dataspacesync-app");
        private static Notification notification = new Notification();
        private static object notificationLock = new object();

        /// <summary>
        /// Creates a Notification by Notificatoin Daemon
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
            if (content == null) {
                content = string.Empty;
            }

            lock(notificationLock) {
                notification.Summary = title;
                notification.Body = content;
                notification.IconName = string.IsNullOrEmpty(iconPath) ? IconName : iconPath;
                notification.Show();
            }
        }
    }
}