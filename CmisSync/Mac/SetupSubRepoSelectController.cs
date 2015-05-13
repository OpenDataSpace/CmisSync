//-----------------------------------------------------------------------
// <copyright file="SetupSubRepoSelectController.cs" company="GRAU DATA AG">
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
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CmisSync.CmisTree;
    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Config;

    using MonoMac.AppKit;
    using MonoMac.Foundation;
    public partial class SetupSubRepoSelectController : MonoMac.AppKit.NSViewController {
        #region Constructors

        // Called when created from unmanaged code
        public SetupSubRepoSelectController(IntPtr handle) : base(handle) {
            this.Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public SetupSubRepoSelectController(NSCoder coder) : base(coder) {
            this.Initialize();
        }

        // Call to load from the XIB/NIB file
        public SetupSubRepoSelectController(SetupController controller) : base("SetupSubRepoSelect", NSBundle.MainBundle) {
            this.Controller = controller;
            this.Initialize();
        }

        // Shared initialization code
        private void Initialize() {
        }

        #endregion

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            Console.WriteLine(this.GetType().ToString() + " disposed " + disposing.ToString());
        }

        private List<RootFolder> Repositories;
        private CmisTreeDataSource DataSource;
        private OutlineViewDelegate DataDelegate;
        private Dictionary<string, AsyncNodeLoader> Loader;

        public override void AwakeFromNib() {
            base.AwakeFromNib();

            bool firstRepo = true;
            this.Repositories = new List<RootFolder>();
            Loader = new Dictionary<string, AsyncNodeLoader>();
            foreach (var repository in Controller.repositories) {
                RootFolder repo = new RootFolder() {
                    Name = repository.Name,
                    Id = repository.Id,
                    Address = this.Controller.saved_address.ToString()
                };
                this.Repositories.Add(repo);
                if (firstRepo) {
                    repo.Selected = true;
                    firstRepo = false;
                } else {
                    repo.Selected = false;
                }

                CmisRepoCredentials cred = new CmisRepoCredentials() {
                    UserName = this.Controller.saved_user,
                    Password = this.Controller.saved_password,
                    Address = this.Controller.saved_address,
                    RepoId = repository.Id
                };
                //  GUI workaround to remove ignore folder {{
                //AsyncNodeLoader asyncLoader = new AsyncNodeLoader(repo, cred, PredefinedNodeLoader.LoadSubFolderDelegate, PredefinedNodeLoader.CheckSubFolderDelegate);
                //Loader.Add(repo.Id, asyncLoader);
                repo.Status = LoadingStatus.DONE;
                //  GUI workaround to remove ignore folder }}

            }

            this.DataSource = new CmisTree.CmisTreeDataSource(this.Repositories);
            this.DataDelegate = new OutlineViewDelegate ();
            Outline.DataSource = this.DataSource;
            Outline.Delegate = this.DataDelegate;

            ContinueButton.Enabled = this.Repositories.Count > 0;
//            ContinueButton.KeyEquivalent = "\r";

            this.BackButton.Title = Properties_Resources.Back;
            this.CancelButton.Title = Properties_Resources.Cancel;
            this.ContinueButton.Title = Properties_Resources.Continue;

            this.InsertEvent();

            //  must be called after InsertEvent()
            //  GUI workaround to remove ignore folder {{
            //foreach (RootFolder repo in Repositories) {
            //    Loader [repo.Id].Load (repo);
            //}
            //  GUI workaround to remove ignore folder }}
        }

        SetupController Controller;

        void OutlineUpdate() {
            InvokeOnMainThread(delegate {
                foreach (RootFolder root in Repositories) {
                    DataSource.UpdateCmisTree(root);
                }

                for (int i = 0; i < Outline.RowCount; ++i) {
                    Outline.ReloadItem(Outline.ItemAtRow(i));
                }
            });
        }

        void OutlineSelected(NSCmisTree cmis, int selected) {
            InvokeOnMainThread(delegate {
                RootFolder selectedRoot = null;
                foreach (RootFolder root in Repositories) {
                    Node node = cmis.GetNode(root);
                    if (node != null) {
                        if (node.Parent == null && node.Selected == false) {
                            selectedRoot = root;
                        }

                        node.Selected = selected != 0;
                        DataSource.UpdateCmisTree(root);
                    }
                }

                if (selectedRoot != null) {
                    foreach (RootFolder root in Repositories) {
                        if (root != selectedRoot) {
                            root.Selected = false;
                            DataSource.UpdateCmisTree(root);
                        }
                    }

                    Outline.ReloadData();
                } else {
                    for (int i = 0; i < Outline.RowCount; ++i) {
                        Outline.ReloadItem(Outline.ItemAtRow(i));
                    }
                }
            });
        }

        void OutlineItemExpanded(NSNotification notification) {
            InvokeOnMainThread(delegate {
                NSCmisTree cmis = notification.UserInfo["NSObject"] as NSCmisTree;
                if (cmis == null) {
                    Console.WriteLine("ItemExpanded Error");
                    return;
                }

                NSCmisTree cmisRoot = cmis;
                while (cmisRoot.Parent != null) {
                    cmisRoot = cmisRoot.Parent;
                }

                RootFolder root = this.Repositories.Find(x => x.Name.Equals(cmisRoot.Name));
                if (root == null) {
                    Console.WriteLine("ItemExpanded find root Error");
                    return;
                }

                Node node = cmis.GetNode(root);
                if (node == null) {
                    Console.WriteLine("ItemExpanded find node Error");
                    return;
                }

                this.Loader[root.Id].Load(node);
            });
        }

        private void InsertEvent() {
            this.DataSource.SelectedEvent += OutlineSelected;
            this.DataDelegate.ItemExpanded += OutlineItemExpanded;
            foreach (AsyncNodeLoader task in this.Loader.Values)
                task.UpdateNodeEvent += OutlineUpdate;
        }

        void RemoveEvent() {
            this.DataSource.SelectedEvent -= OutlineSelected;
            this.DataDelegate.ItemExpanded -= OutlineItemExpanded;
            foreach (AsyncNodeLoader task in Loader.Values) {
                task.UpdateNodeEvent -= OutlineUpdate;
            }
        }

        partial void OnBack(MonoMac.Foundation.NSObject sender) {
            RemoveEvent();
            foreach (AsyncNodeLoader task in Loader.Values) {
                task.Cancel();
            }

            Controller.BackToPage1();
        }

        partial void OnCancel(MonoMac.Foundation.NSObject sender) {
            RemoveEvent();
            foreach (AsyncNodeLoader task in Loader.Values) {
                task.Cancel();
            }

            Controller.PageCancelled();
        }

        partial void OnContinue(MonoMac.Foundation.NSObject sender) {
            RootFolder root = Repositories.Find(x=>(x.Selected != false));
            if (root != null) {
                RemoveEvent();
                foreach (AsyncNodeLoader task in Loader.Values) {
                    task.Cancel();
                }

                Controller.saved_repository = root.Id;
                List<string> ignored = NodeModelUtils.GetIgnoredFolder(root);
                List<string> selected = NodeModelUtils.GetSelectedFolder(root);
                Controller.Add2PageCompleted(root.Id, root.Path, ignored.ToArray(), selected.ToArray());
            }
        }

        // strongly typed view accessor
        public new SetupSubRepoSelect View {
            get {
                return (SetupSubRepoSelect)base.View;
            }
        }
    }
}