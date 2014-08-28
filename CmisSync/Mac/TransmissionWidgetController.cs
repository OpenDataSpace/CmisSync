
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
            HandleDeleteTransmissionEvent (item);
            HandleInsertTransmissionEvent (item);
            Controller.ShowTransmissionList ();
        }

        private void HandleDeleteTransmissionEvent (TransmissionItem item)
        {
            lock (lockTransmissionItems) {
                bool removeItem = false;
                do {
                    removeItem = false;
                    foreach (TransmissionItem transmissionItem in TransmissionItems) {
                        if (transmissionItem.FullPath == item.FullPath) {
                            TransmissionItems.Remove (transmissionItem);
                            removeItem = true;
                            break;
                        }
                    }
                } while(removeItem);
            }
        }

        private void HandleInsertTransmissionEvent (TransmissionItem item)
        {
            lock (lockTransmissionItems) {
                TransmissionItems.Insert (0, item);
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

        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();
            TableView.DataSource = new TransmissionDataSource (Controller);
            FinishButton.Title = Properties_Resources.Finish;
            Controller.ShowTransmissionListEvent += delegate
            {
                using (var a = new NSAutoreleasePool ())
                {
                    BeginInvokeOnMainThread (delegate {
                        TableView.ReloadData();
                    });
                }
            };
        }

        partial void OnFinish (MonoMac.Foundation.NSObject sender)
        {
            Controller.HideWindow();
        }

        //strongly typed window accessor
        public new TransmissionWidget Window {
            get {
                return (TransmissionWidget)base.Window;
            }
        }
    }
}

