using System;
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Collections.Generic;

using CmisSync.Lib;

using DotCMIS.Client;

namespace CmisSync.Lib.Data
{
    [Serializable]
    public abstract class AbstractMappedObject
    {
        public AbstractMappedObject( string localSyncTargetPath, string remoteSyncTargetPath) {
            if (String.IsNullOrEmpty(localSyncTargetPath))
                throw new ArgumentException("Given local sync target path is null or empty");
            if (String.IsNullOrEmpty(remoteSyncTargetPath))
                throw new ArgumentException("Given remote sync target path is null or empty");
            LocalSyncTargetPath = localSyncTargetPath;
            RemoteSyncTargetPath = remoteSyncTargetPath;
        }
        public virtual string RemoteObjectId { get; set; }

        public virtual string LastChangeToken { get; set; }

        [DefaultValue(null)]
        public virtual DateTime? LastRemoteWriteTimeUtc { get; set; }

        [DefaultValue(null)]
        public virtual DateTime? LastLocalWriteTimeUtc { get; set; }

        public virtual byte[] LastChecksum { get; set; }

        public virtual string ChecksumAlgorithmName { get; set; }

        public virtual string RemoteSyncTargetPath { get; private set; }

        public virtual string LocalSyncTargetPath { get; private set; }

        public virtual string Name { get; set; }

        public virtual string Description { get; set; }

        public abstract bool ExistsLocally ();
    }

    [Serializable]
    public class MappedFolder : AbstractMappedObject
    {

        public MappedFolder Parent { get; set; }

        private List<AbstractMappedObject> children = new List<AbstractMappedObject> ();

        public List<AbstractMappedObject> Children { get { return children; } set { this.children = value; } }

        public override bool ExistsLocally ()
        {
            return Directory.Exists (GetLocalPath ());
        }

        public string GetLocalPath ()
        {
            if (Parent == null)
                return LocalSyncTargetPath;
            else {
                string path = Name;
                MappedFolder p = Parent;
                while (p.Parent != null) {
                    path = Path.Combine (p.Name, path);
                    p = p.Parent;
                }
                return Path.Combine (LocalSyncTargetPath, path);
            }
        }

        public MappedFolder( string localSyncTargetPath, string remoteSyncTargetPath) : base(localSyncTargetPath, remoteSyncTargetPath) {
            Name = new DirectoryInfo(LocalSyncTargetPath).Name;
        }

        public MappedFolder ( MappedFolder parent, string name ) : base(parent.LocalSyncTargetPath, parent.RemoteSyncTargetPath) {
            if(parent == null)
                throw new ArgumentNullException("Given parent is null");
            if(String.IsNullOrEmpty(name))
                throw new ArgumentException("Given name is null or empty");
            Parent = parent;
            Name = name;
        }
    }

    [Serializable]
    public class MappedFile : AbstractMappedObject
    {
        public List<MappedFolder> Parents { get; set; }

        [DefaultValue(-1)]
        public long LastFileSize { get; set; }

        public MappedFile (MappedFolder parent, params MappedFolder[] parents) : base(parent.LocalSyncTargetPath, parent.RemoteSyncTargetPath)
        {
            Parents = new List<MappedFolder>();
            Parents.Add (parent);
            if (parents != null)
                Parents.AddRange (parents);
        }

        public override bool ExistsLocally ()
        {
            if (Parents.Count != 1)
                throw new ArgumentOutOfRangeException (String.Format ("Only if one parent exists, this method could return the corect answer, but there are {0} parents", Parents.Count));
            return File.Exists (Path.Combine (Parents [0].GetLocalPath (), Name));
        }

        public bool ExistsLocally (MappedFolder parent)
        {
            if (this.Parents.Contains (parent))
                return File.Exists (parent.GetLocalPath ());
            else 
                return false;
        }

        public string GetLocalPath (MappedFolder parent)
        {
            return Path.Combine (parent.GetLocalPath (), Name);
        }

        public string GetLocalPath ()
        {
            return Path.Combine (Parents [0].GetLocalPath (), Name);
        }

        public bool HasBeenChangedLocally (out byte[] newChecksum)
        {
            FileInfo file = new FileInfo (GetLocalPath ());
            newChecksum = null;
            // Check Last Write Access if available
            if (this.LastLocalWriteTimeUtc == null) {
                return file.Exists;
            } else {
                if (file.LastWriteTimeUtc.Equals (this.LastLocalWriteTimeUtc)) {
                    newChecksum = this.LastChecksum;
                    return false;
                }
            }

            if (file.Length != this.LastFileSize)
                return true;

            // Calculate current checksum.
            try {
                newChecksum = Crypto.CalculateChecksum (this.ChecksumAlgorithmName, file);
            } catch (IOException) {
                return true;
            }
            return !newChecksum.Equals (this.LastChecksum);
        }

        public bool HasBeenChangeRemotely (IDocument remoteDocument)
        {
            if (this.RemoteObjectId != null && remoteDocument.Id != this.RemoteObjectId)
                throw new ArgumentException ("Given remote object does not fit to this object");

            // Check ChangeTokens if available
            if (LastChangeToken != null && remoteDocument.ChangeToken != null)
                return !remoteDocument.ChangeToken.Equals (this.LastChangeToken);

            // Check file name equality
            if(!Name.Equals(remoteDocument.Name))
                return true;

            // Check ContentStreamLength if available
            if (remoteDocument.ContentStreamLength != null && remoteDocument.ContentStreamLength != this.LastFileSize)
                return true;

            // If no write time has been set, there must be a change, because there is a remote object
            if (this.LastRemoteWriteTimeUtc == null)
                return true;

            // Check modification dates
            DateTime serverSideModificationDate = ((DateTime)remoteDocument.LastModificationDate).ToUniversalTime ();
            return serverSideModificationDate > this.LastRemoteWriteTimeUtc;
        }
    }
}

