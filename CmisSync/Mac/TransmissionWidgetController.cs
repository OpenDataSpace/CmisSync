//-----------------------------------------------------------------------
// <copyright file="TransmissionWidgetController.cs" company="GRAU DATA AG">
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
namespace CmisSync {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CmisSync.Lib.FileTransmission;

    using MonoMac.Foundation;
    using MonoMac.AppKit;

    public class TransmissionDataSource : NSTableViewDataSource {
        object lockTransmissionItems = new object();
        private List<Transmission> TransmissionItems = new List<Transmission>();
        private bool changeAll = false;

        TransmissionController Controller;

        public TransmissionDataSource(TransmissionController controller) {
            Controller = controller;
            Controller.InsertTransmissionEvent += HandleInsertTransmissionEvent;
            Controller.DeleteTransmissionEvent += HandleDeleteTransmissionEvent;
            Controller.UpdateTransmissionEvent += HandleUpdateTransmissionEvent;
        }

        private void HandleUpdateTransmissionEvent(Transmission item) {
            lock (lockTransmissionItems) {
                for (int i = 0; i < TransmissionItems.Count; ++i) {
                    if (TransmissionItems[i].Path == item.Path) {
                        TransmissionItems[i] = item;
                        if (item.Done) {
                            // finished TransmissionItem should put to the tail
                            if (i < TransmissionItems.Count - 1 && !TransmissionItems[i + 1].Done) {
                                TransmissionItems[i] = TransmissionItems[i + 1];
                                TransmissionItems[i + 1] = item;
                                changeAll = true;
                                continue;
                            }
                        }

                        return;
                    }
                }
            }
        }

        private void HandleDeleteTransmissionEvent(Transmission item) {
            lock (lockTransmissionItems) {
                for (int i = TransmissionItems.Count - 1; i >= 0; --i) {
                    if (TransmissionItems[i].Path == item.Path) {
                        TransmissionItems.RemoveAt(i);
                        changeAll = true;
                        return;
                    }
                }
            }
        }

        private void HandleInsertTransmissionEvent(Transmission item) {
            lock (lockTransmissionItems) {
                TransmissionItems.Insert(0, item);
                changeAll = true;
            }
        }

        public Transmission GetTransmissionItem(int row) {
            lock (lockTransmissionItems) {
                if (row < TransmissionItems.Count) {
                    return TransmissionItems[row];
                }
                return null;
            }
        }

        public override NSObject GetObjectValue(NSTableView tableView, NSTableColumn tableColumn, int row) {
            lock (lockTransmissionItems) {
                if (row >= TransmissionItems.Count) {
                    return new NSNull();
                }

                switch (tableColumn.Identifier) {
                case "Repo":
                    return new NSString(TransmissionItems[row].Repository);
                case "Path":
                    return new NSString(TransmissionItems[row].Path);
                case "Status":
                    return new NSString(TransmissionItems[row].Status.ToString());
                case "Progress":
                    return new NSString(TransmissionItems[row].Percent.ToString());
                case "UpdateTime":
                    return new NSString(TransmissionItems[row].LastModification.ToString());
                default:
                    return new NSNull();
                }
            }

            throw new You_Should_Not_Call_base_In_This_Method ();
        }

        public override int GetRowCount(NSTableView tableView) {
            lock (lockTransmissionItems) {
                return TransmissionItems.Count;
            }
        }

        public void UpdateTableView(NSTableView tableView, Transmission item) {
            if (changeAll) {
                changeAll = false;
                BeginInvokeOnMainThread(delegate {
                    tableView.ReloadData();
                });
                return;
            }

            if (item == null) {
                return;
            }

            lock (lockTransmissionItems) {
                for (int i = 0; i < TransmissionItems.Count; ++i) {
                    if (TransmissionItems[i].Path == item.Path) {
                        BeginInvokeOnMainThread(delegate {
                            tableView.ReloadData(new NSIndexSet(i), NSIndexSet.FromArray(new int[]{ 0, 1, 2, 3, 4}));
                        });
                        return;
                    }
                }
            }
        }
    }

    public partial class TransmissionWidgetController : MonoMac.AppKit.NSWindowController {
        #region Constructors

        // Called when created from unmanaged code
        public TransmissionWidgetController(IntPtr handle): base(handle) {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public TransmissionWidgetController(NSCoder coder): base(coder) {
            Initialize();
        }
        
        // Call to load from the XIB/NIB file
        public TransmissionWidgetController(): base("TransmissionWidget") {
            Initialize();
        }
        
        // Shared initialization code
        void Initialize() {
            Controller.ShowWindowEvent += delegate {
                using (var a = new NSAutoreleasePool()) {
                    InvokeOnMainThread(delegate {
                        Window.OrderFrontRegardless();
                    });
                }
            };
            Controller.HideWindowEvent += delegate {
                using (var a = new NSAutoreleasePool()) {
                    InvokeOnMainThread(delegate {
                        Window.PerformClose(this);
                    });
                }
            };
        }

        #endregion

        private TransmissionController Controller = new TransmissionController();
        TransmissionDataSource DataSource;

        public override void AwakeFromNib() {
            base.AwakeFromNib();

            TableColumnRepo.HeaderCell.Title = Properties_Resources.TransmissionTitleRepo;
            TableColumnPath.HeaderCell.Title = Properties_Resources.TransmissionTitlePath;
            TableColumnStatus.HeaderCell.Title = Properties_Resources.TransmissionTitleStatus;
            TableColumnProgress.HeaderCell.Title = Properties_Resources.TransmissionTitleProgress;
            TableColumnUpdateTime.HeaderCell.Title = Properties_Resources.TransmissionTitleLastChange;
            FinishButton.Title = Properties_Resources.Close;

            DataSource = new TransmissionDataSource(Controller);
            TableView.DataSource = DataSource;

            TableView.ShouldSelectRow += delegate(NSTableView tableView, int row) {
                return true;
            };
            TableView.SelectionDidChange += HandleSelectionDidChange;
            TableView.SelectionShouldChange += delegate(NSTableView tableView) {
                return true;
            };
            TableView.AllowsEmptySelection = true;
            TableView.AllowsMultipleSelection = true;

            Controller.ShowTransmissionListEvent += delegate {
                DataSource.UpdateTableView(TableView, null);
                HandleSelectionDidChange(this, new EventArgs());
            };
            Controller.ShowTransmissionEvent += delegate(Transmission item) {
                DataSource.UpdateTableView(TableView, item);
                HandleSelectionDidChange(this, new EventArgs());
            };
        }

        void HandleSelectionDidChange (object sender, EventArgs e) {
            using (var a = new NSAutoreleasePool ()) {
                BeginInvokeOnMainThread(delegate {
                    if (TableView.SelectedRowCount > 0) {
                        TableRowMenuOpen.Enabled = false;
                        foreach (int row in TableView.SelectedRows) {
                            var item = DataSource.GetTransmissionItem(row);
                            if (item != null && item.Done) {
                                TableRowMenuOpen.Enabled = true;
                                break;
                            }
                        }

                        TableView.Menu = TableRowContextMenu;
                    } else {
                        TableView.Menu = new NSMenu();
                    }
                });
            };
        }

        partial void OnFinish(MonoMac.Foundation.NSObject sender) {
            Controller.HideWindow();
        }

        partial void OnOpen(MonoMac.Foundation.NSObject sender) {
            if (TableView.SelectedRowCount <= 0) {
                return;
            }

            NSWorkspace.SharedWorkspace.BeginInvokeOnMainThread(delegate {
                foreach (int row in TableView.SelectedRows) {
                    var item = DataSource.GetTransmissionItem(row);
                    if (item!=null && item.Done) {
                        NSWorkspace.SharedWorkspace.OpenFile(item.Path);
                    }
                }
            });
        }

        partial void OnOpenLocation(MonoMac.Foundation.NSObject sender) {
            if (TableView.SelectedRowCount <= 0) {
                return;
            }

            NSWorkspace.SharedWorkspace.BeginInvokeOnMainThread(delegate {
                if (TableView.SelectedRows == null) {
                    return;
                }

                foreach (int row in TableView.SelectedRows) {
                    var item = DataSource.GetTransmissionItem(row);
                    if (item!=null) { 
                        NSWorkspace.SharedWorkspace.OpenFile(System.IO.Path.GetDirectoryName(item.Path));
                    }
                }
            });
        }

        //strongly typed window accessor
        public new TransmissionWidget Window {
            get {
                return (TransmissionWidget)base.Window;
            }
        }
    }
}