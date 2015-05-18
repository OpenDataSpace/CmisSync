
namespace DiagnoseTool {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CmisSync.Lib.Config;
    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.Storage.Database;

    using DBreeze;

    using MonoMac.AppKit;
    using MonoMac.Foundation;

    public partial class MainWindow : MonoMac.AppKit.NSWindow {
        #region Constructors

        // Called when created from unmanaged code
        public MainWindow(IntPtr handle) : base(handle) {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public MainWindow(NSCoder coder) : base(coder) {
            Initialize();
        }

        // Shared initialization code
        void Initialize() {
        }

        #endregion
        public override void AwakeFromNib() {
            base.AwakeFromNib();

            var config = ConfigManager.CurrentConfig;
            this.folderSelection.RemoveAllItems();
            this.output.Editable = false;
            this.RunButton.Activated += (object sender, EventArgs e) => {
                var folder = config.Folders.Find(f => f.DisplayName == this.folderSelection.SelectedItem.Title);
                using (var dbEngine = new DBreezeEngine(folder.GetDatabasePath())) {
                    var storage = new MetaDataStorage(dbEngine, new PathMatcher(folder.LocalPath, folder.RemotePath), false);
                    try {
                        storage.ValidateObjectStructure();
                        this.output.StringValue = string.Format("{0}: DB structure of {1} is fine", DateTime.Now, folder.GetDatabasePath());
                    } catch(Exception ex) {
                        this.output.StringValue = ex.ToString();
                    }
                }
            };
            foreach (var folder in config.Folders) {
                this.folderSelection.AddItem(folder.DisplayName);
            }
        }
    }
}