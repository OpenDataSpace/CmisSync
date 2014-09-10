
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace CmisSync
{
    public class TransmissionDataSource : NSTableViewDataSource
    {
        object lockTransmissionItems = new object();
        private List<TransmissionItem> TransmissionItems = new List<TransmissionItem>();
        private bool changeAll = false;

        TransmissionController Controller;

        public TransmissionDataSource(TransmissionController controller)
        {
            Controller = controller;
            Controller.InsertTransmissionEvent += HandleInsertTransmissionEvent;
            Controller.DeleteTransmissionEvent += HandleDeleteTransmissionEvent;
            Controller.UpdateTransmissionEvent += HandleUpdateTransmissionEvent;
        }

        private void HandleUpdateTransmissionEvent (TransmissionItem item)
        {
            lock (lockTransmissionItems) {
                for (int i = 0; i < TransmissionItems.Count; ++i) {
                    if (TransmissionItems [i].FullPath == item.FullPath) {
                        TransmissionItems [i] = item;
                        if (item.Done) {
                            // finished TransmissionItem should put to the tail
                            if (i < TransmissionItems.Count - 1 && !TransmissionItems [i + 1].Done) {
                                TransmissionItems [i] = TransmissionItems [i + 1];
                                TransmissionItems [i + 1] = item;
                                changeAll = true;
                                continue;
                            }
                        }
                        return;
                    }
                }
            }
        }

        private void HandleDeleteTransmissionEvent (TransmissionItem item)
        {
            lock (lockTransmissionItems) {
                for (int i = TransmissionItems.Count - 1; i >= 0; --i) {
                    if (TransmissionItems [i].FullPath == item.FullPath) {
                        TransmissionItems.RemoveAt (i);
                        changeAll = true;
                        return;
                    }
                }
            }
        }

        private void HandleInsertTransmissionEvent (TransmissionItem item)
        {
            lock (lockTransmissionItems) {
                TransmissionItems.Insert (0, item);
                changeAll = true;
            }
        }

        public TransmissionItem GetTransmissionItem(int row)
        {
            lock (lockTransmissionItems) {
                if (row < TransmissionItems.Count) {
                    return TransmissionItems [row];
                }
                return null;
            }
        }

        public override NSObject GetObjectValue (NSTableView tableView, NSTableColumn tableColumn, int row)
        {
            lock (lockTransmissionItems) {
                if (row >= TransmissionItems.Count) {
                    return new NSNull ();
                }
                switch (tableColumn.Identifier) {
                case "Repo":
                    return new NSString (TransmissionItems [row].Repo);
                case "Path":
                    return new NSString (TransmissionItems [row].Path);
                case "Status":
                    return new NSString (TransmissionItems [row].Status);
                case "Progress":
                    return new NSString (TransmissionItems [row].Progress);
                default:
                    return new NSNull ();
                }
            }
            throw new You_Should_Not_Call_base_In_This_Method ();
        }

        public override int GetRowCount (NSTableView tableView)
        {
            lock (lockTransmissionItems) {
                return TransmissionItems.Count;
            }
        }

        public void UpdateTableView(NSTableView tableView,TransmissionItem item)
        {
            if (changeAll) {
                changeAll = false;
                BeginInvokeOnMainThread (delegate
                {
                    tableView.ReloadData ();
                });
                return;
            }

            if (item == null) {
                return;
            }

            lock (lockTransmissionItems) {
                for (int i = 0; i < TransmissionItems.Count; ++i) {
                    if (TransmissionItems [i].FullPath == item.FullPath) {
                        BeginInvokeOnMainThread (delegate
                        {
                            tableView.ReloadData (new NSIndexSet (i), NSIndexSet.FromArray (new int[]{ 0, 1, 2, 3 }));
                        });
                        return;
                    }
                }
            }
        }

    }

    public partial class TransmissionWidgetController : MonoMac.AppKit.NSWindowController
    {
        #region Constructors

        // Called when created from unmanaged code
        public TransmissionWidgetController (IntPtr handle) : base (handle)
        {
            Initialize ();
        }
        
        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public TransmissionWidgetController (NSCoder coder) : base (coder)
        {
            Initialize ();
        }
        
        // Call to load from the XIB/NIB file
        public TransmissionWidgetController () : base ("TransmissionWidget")
        {
            Initialize ();
        }
        
        // Shared initialization code
        void Initialize ()
        {
            Controller.ShowWindowEvent += delegate
            {
                using (var a = new NSAutoreleasePool ())
                {
                    InvokeOnMainThread (delegate {
                        Window.OrderFrontRegardless ();
                    });
                }
            };
            Controller.HideWindowEvent += delegate
            {
                using (var a = new NSAutoreleasePool ())
                {
                    InvokeOnMainThread (delegate
                    {
                        Window.PerformClose (this);
                    });
                }
            };
        }

        #endregion

        private TransmissionController Controller = new TransmissionController();
        TransmissionDataSource DataSource;

        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();

            TableColumnRepo.HeaderCell.Title = Properties_Resources.TransmissionTitleRepo;
            TableColumnPath.HeaderCell.Title = Properties_Resources.TransmissionTitlePath;
            TableColumnStatus.HeaderCell.Title = Properties_Resources.TransmissionTitleStatus;
            TableColumnProgress.HeaderCell.Title = Properties_Resources.TransmissionTitleProgress;
            FinishButton.Title = Properties_Resources.Finish;

            DataSource = new TransmissionDataSource (Controller);
            TableView.DataSource = DataSource;

            TableView.ShouldSelectRow += delegate(NSTableView tableView, int row)
            {
                return true;
            };
            TableView.SelectionDidChange += HandleSelectionDidChange;
            TableView.SelectionShouldChange += delegate(NSTableView tableView)
            {
                return true;
            };
            TableView.AllowsEmptySelection = true;
            TableView.AllowsMultipleSelection = true;

            Controller.ShowTransmissionListEvent += delegate
            {
                DataSource.UpdateTableView(TableView,null);
                HandleSelectionDidChange(this,new EventArgs());
            };
            Controller.ShowTransmissionEvent += delegate (TransmissionItem item)
            {
                DataSource.UpdateTableView(TableView,item);
                HandleSelectionDidChange(this,new EventArgs());
            };
        }

        void HandleSelectionDidChange (object sender, EventArgs e)
        {
            using (var a = new NSAutoreleasePool ()) {
                BeginInvokeOnMainThread (delegate
                {
                    if (TableView.SelectedRowCount > 0) {
                        TableRowMenuOpen.Enabled = false;
                        foreach (int row in TableView.SelectedRows) {
                            TransmissionItem item = DataSource.GetTransmissionItem (row);
                            if (item != null && item.Done) {
                                TableRowMenuOpen.Enabled = true;
                                break;
                            }
                        }
                        TableView.Menu = TableRowContextMenu;
                    } else {
                        TableView.Menu = new NSMenu ();
                    }
                });
            };
        }

        partial void OnFinish (MonoMac.Foundation.NSObject sender)
        {
            Controller.HideWindow();
        }

        partial void OnOpen (MonoMac.Foundation.NSObject sender)
        {
            if (TableView.SelectedRowCount <= 0) {
                return;
            }
            NSWorkspace.SharedWorkspace.BeginInvokeOnMainThread (delegate
            {
                foreach (int row in TableView.SelectedRows) {
                    TransmissionItem item = DataSource.GetTransmissionItem(row);
                    if (item!=null && item.Done) {
                        NSWorkspace.SharedWorkspace.OpenFile (item.FullPath);
                    }
                }
            });
        }

        partial void OnOpenLocation (MonoMac.Foundation.NSObject sender)
        {
            if (TableView.SelectedRowCount <= 0) {
                return;
            }
            NSWorkspace.SharedWorkspace.BeginInvokeOnMainThread (delegate
            {
                if (TableView.SelectedRows == null) {
                    return;
                }
                foreach (int row in TableView.SelectedRows) {
                    TransmissionItem item = DataSource.GetTransmissionItem(row);
                    if (item!=null) { 
                        NSWorkspace.SharedWorkspace.OpenFile (System.IO.Path.GetDirectoryName(item.FullPath));
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

