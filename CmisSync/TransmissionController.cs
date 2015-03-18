//-----------------------------------------------------------------------
// <copyright file="TransmissionController.cs" company="GRAU DATA AG">
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

namespace CmisSync {
    ﻿using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text;

    using CmisSync.Lib.Config;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.FileTransmission;

    public static class TransmissionExtensions {
        public static void AddRelativePathAndRepository(this Transmission transmission) {
            string fullPath = transmission.Path;
            foreach (RepoInfo repoInfo in ConfigManager.CurrentConfig.Folders) {
                string localFolder = repoInfo.LocalPath.TrimEnd(System.IO.Path.DirectorySeparatorChar) + System.IO.Path.DirectorySeparatorChar.ToString();
                if (fullPath.StartsWith(localFolder)) {
                    transmission.Repository = repoInfo.DisplayName;
                    transmission.RelativePath = fullPath.Substring(localFolder.Length);
                }
            }
        }

        public static bool Done(this Transmission transmission) {
            var status = transmission.Status;
            return status == TransmissionStatus.ABORTED || status == TransmissionStatus.FINISHED;
        }
    }

    public class TransmissionController {
        public event Action ShowWindowEvent = delegate { };
        public event Action HideWindowEvent = delegate { };

        public event Action<Transmission> InsertTransmissionEvent = delegate { };
        public event Action<Transmission> UpdateTransmissionEvent = delegate { };
        public event Action<Transmission> DeleteTransmissionEvent = delegate { };

        public event Action ShowTransmissionListEvent = delegate { };
        public event Action<Transmission> ShowTransmissionEvent = delegate { };

        private List<Transmission> TransmissionList = new List<Transmission>();
        private readonly int TransmissionLimitLeast = 15;
        private HashSet<string> FullPathList = new HashSet<string>();

        public TransmissionController() {
            Program.Controller.ShowTransmissionWindowEvent += ShowWindow;

            Program.Controller.OnTransmissionListChanged += Controller_OnTransmissionListChanged;
        }

        public void ShowWindow() {
            ShowWindowEvent();
        }

        public void HideWindow() {
            HideWindowEvent();
        }

        public void UpdateTransmission(Transmission item) {
            UpdateTransmissionEvent(item);
        }

        public void ShowTransmissionList() {
            ShowTransmissionListEvent();
        }

        public void ShowTransmission(Transmission item) {
            ShowTransmissionEvent(item);
        }

        public class TransmissionCompare : IComparer<Transmission> {
            public int Compare(Transmission x, Transmission y) {
                if (x.Done() != y.Done()) {
                    return x.Done() ? 1 : -1;
                }

                if (x.LastModification == y.LastModification) {
                    return 0;
                }

                if (x.LastModification > y.LastModification) {
                    return -1;
                }

                return 1;
            }
        }

        private void Controller_OnTransmissionListChanged() {
            int transmissionLimit = ConfigManager.CurrentConfig.TransmissionLimit;
            if (transmissionLimit < TransmissionLimitLeast) {
                transmissionLimit = TransmissionLimitLeast;
            }

            var transmissions = Program.Controller.ActiveTransmissions();
            foreach (var transmission in transmissions) {
                string fullPath = transmission.Path;
                if (FullPathList.Contains(fullPath)) {
                    TransmissionItem itemOld = TransmissionList.Find(t => t.FullPath == fullPath);
                    if (!itemOld.Done) {
                        continue;
                    }
                    DeleteTransmissionEvent(itemOld);
                    TransmissionList.Remove(itemOld);
                    FullPathList.Remove(fullPath);
                }

                FullPathList.Add(fullPath);
                TransmissionItem item = new TransmissionItem(transmission);
                TransmissionList.Insert(0, item);
                InsertTransmissionEvent(item);

                //  register TransmissionController.UpdateTransmissionEvent
                item.Controller = this;
            }

            if (FullPathList.Count > transmissionLimit) {
                TransmissionList.Sort(new TransmissionCompare());
                for (int i = FullPathList.Count - 1; i >= 0 && TransmissionList.Count > transmissionLimit; --i) {
                    TransmissionItem item = TransmissionList[i];
                    if (!item.Done) {
                        break;
                    }

                    //  un-register TransmissionController.UpdateTransmissionEvent
                    item.Controller = null;

                    DeleteTransmissionEvent(item);
                    TransmissionList.RemoveAt(i);
                    FullPathList.Remove(item.FullPath);

                    //  un-register FileTransmissionEvent.TransmissionStatus
                    item.Dispose();
                }
            }

            ShowTransmissionListEvent();
        }
    }
}