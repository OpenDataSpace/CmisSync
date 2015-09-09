//-----------------------------------------------------------------------
// <copyright file="ObjectExtenders.cs" company="GRAU DATA AG">
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
﻿
namespace CmisSync.Lib.Cmis.ConvenienceExtenders {
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Config;

    using DotCMIS;
    using DotCMIS.Client;

    /// <summary>
    /// DotCMIS Object extenders.
    /// </summary>
    public static class ObjectExtenders {
        /// <summary>
        /// Removes the sync ignore flags of the given device Ids from the given cmis object.
        /// </summary>
        /// <param name="obj">Remote CMIS Object.</param>
        /// <param name="deviceIds">Device identifiers which should be removed from ignore list.</param>
        public static void RemoveSyncIgnore(this ICmisObject obj, params Guid[] deviceIds) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            if (deviceIds == null) {
                deviceIds = new Guid[0];
            }

            string[] ids = new string[deviceIds.Length];
            for (int i = 0; i < deviceIds.Length; i++) {
                ids[i] = deviceIds[i].ToString().ToLower();
            }

            obj.RemoveSyncIgnore(ids);
        }

        /// <summary>
        /// Removes the sync ignore flags of the given device Ids from the given cmis object.
        /// </summary>
        /// <param name="obj">Remote CMIS Object.</param>
        /// <param name="deviceIds">Device identifiers which should be removed from ignore list.</param>
        public static void RemoveSyncIgnore(this ICmisObject obj, params string[] deviceIds) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            var ids = obj.SecondaryObjectTypeIds();
            if (ids.Contains("gds:sync")) {
                var devices = obj.IgnoredDevices();
                if (deviceIds == null || deviceIds.Length == 0) {
                    if (devices.Remove("*") ||
                        devices.Remove(ConfigManager.CurrentConfig.DeviceId.ToString().ToLower())) {
                        if (devices.Count > 0) {
                            Dictionary<string, object> properties = new Dictionary<string, object>();
                            properties.Add("gds:ignoreDeviceIds", devices);
                            obj.UpdateProperties(properties, true);
                        } else {
                            obj.RemoveAllSyncIgnores();
                        }
                    }
                } else {
                    bool changed = false;
                    foreach (var removeId in deviceIds) {
                        if (devices.Remove(removeId)) {
                            changed = true;
                        }
                    }

                    if (changed) {
                        if (devices.Count > 0) {
                            Dictionary<string, object> properties = new Dictionary<string, object>();
                            properties.Add("gds:ignoreDeviceIds", devices);
                            obj.UpdateProperties(properties, true);
                        } else {
                            obj.RemoveAllSyncIgnores();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes all sync ignores flags from the given cmis object
        /// </summary>
        /// <param name="obj">Remote CMIS Object.</param>
        public static void RemoveAllSyncIgnores(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            var properties = new Dictionary<string, object>();
            var ids = obj.SecondaryObjectTypeIds();
            if (ids.Remove("gds:sync")) {
                properties.Add(PropertyIds.SecondaryObjectTypeIds, ids);
                obj.UpdateProperties(properties, true);
            }
        }

        /// <summary>
        /// Lists all Ignored devices.
        /// </summary>
        /// <returns>The devices.</returns>
        /// <param name="obj">Cmis object.</param>
        public static IList<string> IgnoredDevices(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            IList<string> result = new List<string>();
            if (obj.Properties != null) {
                foreach (var ignoredProperty in obj.Properties) {
                    if (ignoredProperty.Id.Equals("gds:ignoreDeviceIds")) {
                        if (ignoredProperty.Values != null) {
                            foreach (var device in ignoredProperty.Values) {
                                result.Add((device as string).ToLower());
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Secondary object type identifiers.
        /// </summary>
        /// <returns>The object type identifiers.</returns>
        /// <param name="obj">Cmis object.</param>
        public static IList<string> SecondaryObjectTypeIds(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            IList<string> ids = new List<string>();
            if (obj.SecondaryTypes != null) {
                foreach (var secondaryType in obj.SecondaryTypes) {
                    ids.Add(secondaryType.Id);
                }
            }

            return ids;
        }

        /// <summary>
        /// Indicates if all children are ignored.
        /// </summary>
        /// <returns><c>true</c>, if all children are ignored, <c>false</c> otherwise.</returns>
        /// <param name="obj">Remote cmis object.</param>
        public static bool AreAllChildrenIgnored(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            var devices = obj.IgnoredDevices();
            if (devices.Contains("*") || devices.Contains(ConfigManager.CurrentConfig.DeviceId.ToString().ToLower())) {
                return true;
            } else {
                return false;
            }
        }
    }
}