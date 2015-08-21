//-----------------------------------------------------------------------
// <copyright file="Utils.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Text.RegularExpressions;

#if __MonoCS__
    using Mono.Unix.Native;
#endif

    using log4net;

    using Newtonsoft.Json;

    /// <summary>
    /// Static methods that are useful in the context of synchronization.
    /// </summary>
    public static class Utils {
        /// <summary>
        /// Log 4 net logger.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Utils));

        /// <summary>
        /// Regular expression to check whether a file name is valid or not.
        /// </summary>
        private static Regex invalidFileNameRegex = new Regex("[" + Regex.Escape(new string(Path.GetInvalidFileNameChars()) + "\"?:/\\|<>*") + "]");

        /// <summary>
        /// Regular expression to check whether a filename is valid or not.
        /// </summary>
        private static Regex invalidFolderNameRegex = new Regex("[" + Regex.Escape(new string(Path.GetInvalidPathChars()) + "\"?:/\\|<>*") + "]");

        /// <summary>
        /// Check whether the current user has write permission to the specified path.
        /// </summary>
        /// <param name="path">Absolut path to be checked for permissions</param>
        /// <returns><c>true</c> if the write permission is granted, otherwise <c>false</c></returns>
        public static bool HasWritePermissionOnDir(string path) {
            var writeAllow = false;
            var writeDeny = false;
            try {
                var accessControlList = Directory.GetAccessControl(path);
                if (accessControlList == null) {
                    return false;
                }

                var accessRules = accessControlList.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
                if (accessRules == null) {
                    return false;
                }

                foreach (System.Security.AccessControl.FileSystemAccessRule rule in accessRules) {
                    if ((System.Security.AccessControl.FileSystemRights.Write & rule.FileSystemRights)
                            != System.Security.AccessControl.FileSystemRights.Write) {
                        continue;
                    }

                    if (rule.AccessControlType == System.Security.AccessControl.AccessControlType.Allow) {
                        writeAllow = true;
                    } else if (rule.AccessControlType == System.Security.AccessControl.AccessControlType.Deny) {
                        writeDeny = true;
                    }
                }
            } catch (System.PlatformNotSupportedException) {
#if __MonoCS__
                writeAllow = (0 == Syscall.access(path, AccessModes.W_OK));
#endif
            } catch(System.UnauthorizedAccessException) {
                var permission = new FileIOPermission(FileIOPermissionAccess.Write, path);
                var permissionSet = new PermissionSet(PermissionState.None);
                permissionSet.AddPermission(permission);
                return permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
            }

            return writeAllow && !writeDeny;
        }

        /// <summary>
        /// <para>Creates a log-string from the Exception.</para>
        /// <para>The result includes the stacktrace, innerexception et cetera, separated by <seealso cref="Environment.NewLine"/>.</para>
        /// <para>Code from http://www.extensionmethod.net/csharp/exception/tologstring</para>
        /// </summary>
        /// <param name="ex">The exception to create the string from.</param>
        /// <returns>A formated log string</returns>
        public static string ToLogString(this Exception ex) {
            StringBuilder msg = new StringBuilder();
 
            if (ex != null) {
                string newline = Environment.NewLine;

                Exception orgEx = ex;
 
                msg.Append("Exception:");
        
                msg.Append(newline);
                while (orgEx != null) {
                    msg.Append(orgEx.Message);
                    msg.Append(newline);
                    orgEx = orgEx.InnerException;
                }
 
                if (ex.Data != null) {
                    foreach (object i in ex.Data) {
                        msg.Append("Data :");
                        msg.Append(i.ToString());
                        msg.Append(newline);
                    }
                }
 
                if (ex.StackTrace != null) {
                    msg.Append("StackTrace:");
                    msg.Append(newline);
                    msg.Append(ex.StackTrace);
                    msg.Append(newline);
                }
 
                if (ex.Source != null) {
                    msg.Append("Source:");
                    msg.Append(newline);
                    msg.Append(ex.Source);
                    msg.Append(newline);
                }
 
                if (ex.TargetSite != null) {
                    msg.Append("TargetSite:");
                    msg.Append(newline);
                    msg.Append(ex.TargetSite.ToString());
                    msg.Append(newline);
                }
 
                Exception baseException = ex.GetBaseException();
                if (baseException != null) {
                    msg.Append("BaseException:");
                    msg.Append(newline);
                    msg.Append(ex.GetBaseException());
                }
            }

            return msg.ToString();
        }

        /// <summary>
        /// Check whether the file is worth syncing or not.
        /// Files that are not worth syncing include temp files, locks, etc.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="ignoreWildcards"></param>
        /// <returns></returns>
        public static bool WorthSyncing(string filename, IList<string> ignoreWildcards) {
            if (null == filename) {
                return false;
            }

            if (IsInvalidFileName(filename)) {
                return false;
            }

            ignoreWildcards = ignoreWildcards ?? new List<string>();

            foreach (var wildcard in ignoreWildcards) {
                var regex = IgnoreLineToRegex(wildcard);
                if (regex.IsMatch(filename)) {
                    Logger.Debug(string.Format("Unworth syncing: \"{0}\" because it matches \"{1}\"", filename, wildcard));
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check whether a file name is valid or not.
        /// </summary>
        public static bool IsInvalidFileName(string name) {
            bool ret = invalidFileNameRegex.IsMatch(name);
            if (ret) {
                Logger.Debug(string.Format("The given file name {0} contains invalid patterns", name));
                return ret;
            }

            return ret;
        }

        public static bool IsInvalidFolderName(string name) {
            return IsInvalidFolderName(name, new List<string>());
        }

        /// <summary>
        /// Check whether a folder name is valid or not.
        /// </summary>
        public static bool IsInvalidFolderName(string name, List<string> ignoreWildcards) {
            if (ignoreWildcards == null) {
                throw new ArgumentNullException("ignoreWildcards");
            }

            bool ret = invalidFolderNameRegex.IsMatch(name);
            if (ret) {
                Logger.Debug(string.Format("The given directory name {0} contains invalid patterns", name));
                return ret;
            }

            foreach (string wildcard in ignoreWildcards) {
                if (Utils.IgnoreLineToRegex(wildcard).IsMatch(name)) {
                    Logger.Debug(string.Format("The given folder name \"{0}\" matches the wildcard \"{1}\"", name, wildcard));
                    return true;
                }
            }

            return ret;
        }

        /// <summary>
        /// Format a file size nicely.
        /// Example: 1048576 becomes "1 MB"
        /// </summary>
        /// <param name="byteCount">byte count</param>
        /// <returns>Formatted file size</returns>
        public static string FormatSize(double byteCount) {
            if (byteCount >= 1099511627776) {
                return string.Format("{0:##.##} TB", Math.Round(byteCount / 1099511627776, 1));
            } else if (byteCount >= 1073741824) {
                return string.Format("{0:##.##} GB", Math.Round(byteCount / 1073741824, 1));
            } else if (byteCount >= 1048576) {
                return string.Format("{0:##.##} MB", Math.Round(byteCount / 1048576, 0));
            } else if (byteCount >= 1024) {
                return string.Format("{0:##.##} KB", Math.Round(byteCount / 1024, 0));
            } else {
                return byteCount.ToString() + " bytes";
            }
        }

        /// <summary>
        /// Formats the bandwidth in typical 10 based calculation
        /// </summary>
        /// <returns>
        /// The bandwidth.
        /// </returns>
        /// <param name='bitsPerSecond'>
        /// Bits per second.
        /// </param>
        public static string FormatBandwidth(double bitsPerSecond) {
            if (bitsPerSecond >= (1000d * 1000d * 1000d * 1000d)) {
                return string.Format("{0:##.##} TBit/s", Math.Round(bitsPerSecond / (1000d * 1000d * 1000d * 1000d), 1));
            } else if (bitsPerSecond >= (1000d * 1000d * 1000d)) {
                return string.Format("{0:##.##} GBit/s", Math.Round(bitsPerSecond / (1000d * 1000d * 1000d), 1));
            } else if (bitsPerSecond >= (1000d * 1000d)) {
                return string.Format("{0:##.##} MBit/s", Math.Round(bitsPerSecond / (1000d * 1000d), 1));
            } else if (bitsPerSecond >= 1000d) {
                return string.Format("{0:##.##} KBit/s", Math.Round(bitsPerSecond / 1000d, 1));
            } else {
                return bitsPerSecond.ToString() + " Bit/s";
            }
        }

        /// <summary>
        /// Formats the given double with a leading and tailing zero and appends percent char
        /// </summary>
        /// <returns>
        /// The formatted percent.
        /// </returns>
        /// <param name='p'>
        /// the percentage
        /// </param>
        public static string FormatPercent(double p) {
            return string.Format("{0:0.0} %", Math.Truncate(p * 10) / 10);
        }

        /// <summary>
        /// Format a file size nicely.
        /// Example: 1048576 becomes "1 MB"
        /// </summary>
        /// <param name="byteCount">byte count</param>
        /// <returns>The formatted size</returns>
        public static string FormatSize(long byteCount) {
            return FormatSize((double)byteCount);
        }

        /// <summary>
        /// Formats the bandwidth in typical 10 based calculation
        /// </summary>
        /// <returns>
        /// The bandwidth.
        /// </returns>
        /// <param name='bitsPerSecond'>
        /// Bits per second.
        /// </param>
        public static string FormatBandwidth(long bitsPerSecond) {
            return FormatBandwidth((double)bitsPerSecond);
        }

        /// <summary>
        /// Determines whether a file or directory is a symbolic link.
        /// </summary>
        /// <returns><c>true</c> if the specified path is a symlink; otherwise, <c>false</c>.</returns>
        /// <param name="path">Path to be checked.</param>
        public static bool IsSymlink(string path) {
            FileInfo fileinfo = new FileInfo(path);
            if (fileinfo.Exists) {
                return IsSymlink(fileinfo);
            }

            DirectoryInfo dirinfo = new DirectoryInfo(path);
            if (dirinfo.Exists) {
                return IsSymlink(dirinfo);
            }

            return false;
        }

        /// <summary>
        /// Determines whether this instance is a symlink the specified FileSystemInfo.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance is a symlink the specified fsi; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='fsi'>
        /// If set to <c>true</c> fsi.
        /// </param>
        public static bool IsSymlink(FileSystemInfo fsi) {
            if (fsi == null) {
                throw new ArgumentNullException("fsi");
            }

            return (fsi.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
        }

        /// <summary>
        /// Creates the user agent string for this client.
        /// </summary>
        /// <returns>The user agent.</returns>
        public static string CreateUserAgent(string appName = "DSS") {
            return string.Format(
                "{0}/{1} ({2}; {4}; hostname=\"{3}\")",
                appName,
                Backend.Version,
                Environment.OSVersion.ToString(),
                System.Environment.MachineName,
                CultureInfo.CurrentCulture.Name);
        }

        /// <summary>
        /// Ensures the needed dependencies are available.
        /// </summary>
        public static void EnsureNeededDependenciesAreAvailable() {
            Type[] types = new Type[] {
                typeof(Newtonsoft.Json.JsonConvert),
                typeof(DotCMIS.Client.Impl.Session),
                typeof(DBreeze.DBreezeEngine)
            };
            foreach (var type in types) {
                System.Reflection.Assembly info = type.Assembly;
                Logger.Debug(string.Format("Needed dependency \"{0}\" is available", info));
            }
        }

        /// <summary>
        /// Generates a Regex from the given simple Wildcard string.
        /// </summary>
        /// <returns>The line to regex.</returns>
        /// <param name="line">whildcard line.</param>
        public static Regex IgnoreLineToRegex(string line) {
            return new Regex("^" + Regex.Escape(line).Replace("\\*", ".*").Replace("\\?", ".") + "$");
        }

        /// <summary>
        /// Determines if is repo name is hidden the specified name hiddenRepos.
        /// </summary>
        /// <returns><c>true</c> if is repo name hidden the specified name hiddenRepos; otherwise, <c>false</c>.</returns>
        /// <param name="name">repo name.</param>
        /// <param name="hiddenRepos">Hidden repos.</param>
        public static bool IsRepoNameHidden(string name, params string[] hiddenRepos) {
            foreach (var wildcard in hiddenRepos) {
                if (Utils.IgnoreLineToRegex(wildcard).IsMatch(name)) {
                    return true;
                }
            }

            return false;
        }

        public static string ToHexString(byte[] data) {
            if (data == null) {
                return "(null)";
            } else {
                return BitConverter.ToString(data).Replace("-", string.Empty);
            }
        }

        /// <summary>
        /// Determines whether this instance is valid ISO-8859-15 specified input.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance is valid ISO-8859-15 specified input; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='input'>
        /// If set to <c>true</c> input.
        /// </param>
        public static bool IsValidISO885915(string input) {
            byte[] bytes = Encoding.GetEncoding(28605).GetBytes(input);
            string result = Encoding.GetEncoding(28605).GetString(bytes);
            return string.Equals(input, result);
        }

        /// <summary>
        /// Helper method to return the property name of a given instance property.
        /// https://stackoverflow.com/questions/4266426/c-sharp-how-to-get-the-name-in-string-of-a-class-property
        /// Credits to: https://stackoverflow.com/users/115413/christian-hayter
        /// </summary>
        /// <returns>The property name as string.</returns>
        /// <param name="expr">Expression which points to a property.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static string NameOf<T>(Expression<Func<T>> expr) {
            return ((MemberExpression)expr.Body).Member.Name;
        }

        /// <summary>
        /// Helper method to return the property name of a given class property.
        /// http://stackoverflow.com/questions/8136480/is-it-possible-to-get-an-object-property-name-string-without-creating-the-object
        /// Credits to: http://stackoverflow.com/users/295635/peter
        /// </summary>
        /// <returns>The property name as string.</returns>
        /// <param name="property">Property.</param>
        /// <typeparam name="TModel">The 1st type parameter.</typeparam>
        /// <typeparam name="TProperty">The 2nd type parameter.</typeparam>
        public static string NameOf<TModel, TProperty>(Expression<Func<TModel, TProperty>> property) {
            MemberExpression memberExpression = (MemberExpression)property.Body;
            return memberExpression.Member.Name;
        }
    }
}