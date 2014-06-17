//-----------------------------------------------------------------------
// <copyright file="NodeModel.cs" company="GRAU DATA AG">
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

namespace CmisSync.CmisTree
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    using CmisSync.Lib.Cmis;

    /// <summary>
    /// Tree View Node
    /// </summary>
    public class Node : INotifyPropertyChanged
    {
        private bool threestates = false;
        private Node parent;
        private LoadingStatus status = LoadingStatus.START;
        private ObservableCollection<Node> children = new ObservableCollection<Node>();
        private bool? selected = true;
        private string name;
        private string path;
        private bool illegalFileNameInPath = false;
        private bool enabled = true;
        private NodeLocationType locationType = NodeLocationType.REMOTE;

        /// <summary>
        /// Enumaration of all possible location Types for a Node. It can be Remote, Local, or Both.
        /// </summary>
        public enum NodeLocationType
        {
            /// <summary>
            /// The node does not exists remote or local
            /// </summary>
            NONE,

            /// <summary>
            /// The node exists locally
            /// </summary>
            LOCAL,

            /// <summary>
            /// The node exists remotely
            /// </summary>
            REMOTE,

            /// <summary>
            /// The node exists locally and remotely
            /// </summary>
            BOTH
        }

        /// <summary>
        /// Gets or sets parent node
        /// </summary>
        [DefaultValue(null)]
        public Node Parent {
            get {
                return this.parent;
            }

            set {
                this.parent = value;
                if (this.parent != null && this.parent.IsIllegalFileNameInPath) {
                    this.IsIllegalFileNameInPath = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the ThreeStates capability of Selected Property.
        /// </summary>
        public virtual bool ThreeStates { get { return this.threestates; } set { this.SetField(ref this.threestates, value, "ThreeStates"); } }

        /// <summary>
        /// Loading status of a folder
        /// </summary>
        public LoadingStatus Status { get { return this.status; } set { this.SetField(ref this.status, value, "Status"); } }

        /// <summary>
        /// All subfolder of this folder.
        /// </summary>
        public ObservableCollection<Node> Children { get { return this.children; } }

        /// <summary>
        /// Sets and gets the Selected Property. If true is set, all children will also be selected,
        /// if false, none of the children is selected. If none, at least one of the children is not selected and at least one is selected
        /// </summary>
        public virtual bool? Selected
        {
            get {
                return this.selected;
            }

            set
            {
                if (this.SetField(ref this.selected, value, "Selected")) {
                    if (this.selected == null) {
                        this.ThreeStates = true;
                    } else if (this.selected == true) {
                        this.ThreeStates = false;
                        foreach (Node child in this.Children) {
                            child.Selected = true;
                        }

                        Node p = this.Parent;
                        while (p != null) {
                            if (p.Selected == null || p.Selected == false) {
                                bool allSelected = true;
                                foreach (Node childOfParent in p.Children) {
                                    if (childOfParent.selected != true)
                                    {
                                        allSelected = false;
                                        break;
                                    }
                                }

                                if (allSelected) {
                                    p.Selected = true;
                                } else {
                                    p.ThreeStates = true;
                                    p.Selected = null;
                                }
                            }

                            p = p.Parent;
                        }

                        this.OnPropertyChanged("IsIgnored");
                    } else {
                        this.ThreeStates = false;
                        Node p = this.Parent;
                        while (p != null && p.Selected == true) {
                            p.Selected = null;
                            p = p.Parent;
                        }

                        foreach (Node child in this.Children) {
                            child.Selected = this.selected;
                        }

                        this.OnPropertyChanged("IsIgnored");
                    }
                }
            }
        }

        /// <summary>
        /// The name of the folder
        /// </summary>
        public string Name { get { return this.name; } set { this.SetField(ref this.name, value, "Name"); } }

        /// <summary>
        /// The absolut path of the folder
        /// </summary>
        public virtual string Path { get { return this.path; } set { this.SetField(ref this.path, value, "Path"); } }

        /// <summary>
        /// Sets and gets the Ignored status of a folder
        /// </summary>
        public bool IsIgnored { get { return this.Selected == false; } }

        /// <summary>
        /// If the path or name contains any illegal Pattern, switch prevends from synchronization, this property is set to true
        /// </summary>
        public bool IsIllegalFileNameInPath { get { return this.illegalFileNameInPath; } 
            set {
                this.SetField(ref this.illegalFileNameInPath, value, "IsIllegalFileNameInPath");
                if (this.illegalFileNameInPath) {
                    foreach (Node child in this.children) {
                        child.IsIllegalFileNameInPath = true;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether node is enable or not.
        /// If a state is changed, also its child node are set to the same state.
        /// </summary>
        public bool Enabled
        {
            get { 
                return this.enabled;
            }

            set {
                if (this.SetField(ref this.enabled, value, "Enabled")) {
                    foreach (Node child in this.children) {
                        child.Enabled = this.enabled;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a folder is expanded or not.
        /// </summary>
        public bool Expanded { get; set; }

        /// <summary>
        /// The location type of a folder can be any NodeLocationType
        /// </summary>
        public NodeLocationType LocationType { get { return this.locationType; } set { this.SetField(ref this.locationType, value, "LocationType"); } }

        /// <summary>
        /// Gets or sets the Tooltip informations about this node. It returns the Path of the node
        /// </summary>
        public virtual string ToolTip { get { return this.Path; } }

        /// <summary>
        /// Add a location type
        /// <c>NodeLocationType.REMOTE</c> + <c>NodeLocationType.LOCAL</c> = <c>NodeLocationType.BOTH</c>
        /// </summary>
        /// <param name="type"></param>
        public void AddType(NodeLocationType type)
        {
            switch (this.locationType)
            {
            case NodeLocationType.NONE:
                this.LocationType = type;
                break;
            case NodeLocationType.LOCAL:
                if (type == NodeLocationType.REMOTE || type == NodeLocationType.BOTH) {
                    this.LocationType = NodeLocationType.BOTH;
                }

                break;
            case NodeLocationType.REMOTE:
                if (type == NodeLocationType.LOCAL || type == NodeLocationType.BOTH) {
                    this.LocationType = NodeLocationType.BOTH;
                }

                break;
            }
        }

        // boiler-plate
        /// <summary>
        /// If any property changes, this event will be informed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Execute this if the property with the given propertyName has been changed.
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Helper Method to change a property and this method informs the PropertyChangeEventHandler
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) {
                return false;
            } 

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }

    /// <summary>
    /// Root node for a synchronized folder. It contains the local and the remote path
    /// </summary>
    public class RootFolder : Node
    {

        private string address;

        /// <summary>
        /// Gets and sets the unique repository id. A change would not be propagated to listener
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Readonly Path of the repository.
        /// </summary>
        public override string Path { get { return "/"; } }

        private string localPath;

        /// <summary>
        /// Local path to the synchronization root folder
        /// </summary>
        public string LocalPath { get { return this.localPath; } set { this.SetField(ref this.localPath, value, "LocalPath"); } }

        /// <summary>
        /// The URL of the repository. 
        /// </summary>
        public string Address { get { return this.address; } set { this.SetField(ref this.address, value, "Address"); } }

        /// <summary>
        /// Tooltip informations about this repo. It returns the Id and the address of the repo
        /// </summary>
        public override string ToolTip { get { return "URL: \"" + this.Address + "\"\r\nRepository ID: \"" + this.Id + "\""; } }

        /// <summary>
        /// Overrides the ThreeStates base method to read only
        /// </summary>
        public override bool ThreeStates {
            get {
                return false;
            }

            set {
                base.ThreeStates = value == false ? value : false;
            }
        }

        /// <summary>
        /// Overrides the selection mode of node by returning only false and true
        /// Other possiblities could be possible in the future, but at the moment, only selected or not are valid results
        /// </summary>
        public override bool? Selected { get { return base.Selected != false; } set { base.Selected = value; } }
    }

    /// <summary>
    /// Folder data structure for WPF Control
    /// </summary>
    public class Folder : Node
    {
        /// <summary>
        /// Default constructor. All properties must be manually set.
        /// </summary>
        public Folder() { }

        /// <summary>
        /// Get folder from <c>SubFolder</c> for the path
        /// </summary>
        public static Folder GetSubFolder(string path, Folder f)
        {
            foreach (Folder folder in f.Children) {
                if (folder.Path.Equals(f.Path)) {
                    return folder;
                }
            }

            return null;
        }

        /// <summary>
        /// Return the folder list when <c>Selected</c> is <c>false</c>
        /// </summary>
        /// <returns></returns>
        public static List<string> GetIgnoredFolder(Folder f)
        {
            List<string> result = new List<string>();
            if (f.IsIgnored) {
                result.Add(f.Path);
            } else {
                foreach (Folder child in f.Children) {
                    result.AddRange(GetIgnoredFolder(child));
                }
            }

            return result;
        }

        /// <summary>
        /// Return the folder list when <c>Selected</c> is <c>true</c>
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSelectedFolder(Folder f)
        {
            List<string> result = new List<string>();
            if (f.Selected == true) {
                result.Add(f.Path);
            } else {
                foreach (Folder child in f.Children) {
                    result.AddRange(GetSelectedFolder(child));
                }
            }

            return result;
        }
    }
}