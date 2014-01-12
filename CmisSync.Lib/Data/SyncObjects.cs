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
    public abstract class AbstractSyncObject
    {
        public string RemoteObjectId { get; set; }

        public string LastChangeToken { get; set; }

        [DefaultValue(null)]
        public DateTime? LastRemoteWriteTimeUtc { get; set; }

        [DefaultValue(null)]
        public DateTime? LastLocalWriteTimeUtc { get; set; }

        public byte[] LastChecksum { get; set; }

        public string ChecksumAlgorithmName { get; set; }

        public string RemoteSyncTargetPath { get; set; }

        public string LocalSyncTargetPath { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public abstract bool ExistsLocally ();
    }

    [Serializable]
    public class SyncFolder : AbstractSyncObject
    {

        public SyncFolder Parent { get; set; }

        private List<AbstractSyncObject> children = new List<AbstractSyncObject> ();

        public List<AbstractSyncObject> Children { get { return children; } set { this.children = value; } }

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
                SyncFolder p = Parent;
                while (p.Parent != null) {
                    path = Path.Combine (p.Name, path);
                    p = p.Parent;
                }
                return Path.Combine (LocalSyncTargetPath, path);
            }
        }
    }

    [Serializable]
    public class SyncFile : AbstractSyncObject
    {
        public List<SyncFolder> Parents { get; set; }

        [DefaultValue(-1)]
        public long LastFileSize { get; set; }

        public SyncFile (SyncFolder parent, params SyncFolder[] parents)
        {
            if (parent == null)
                throw new ArgumentNullException ("Given parent is null");
            Parents.Add (parent);
            Parents.AddRange (parents);
            RemoteSyncTargetPath = parent.RemoteSyncTargetPath;
            LocalSyncTargetPath = parent.LocalSyncTargetPath;
        }

        public override bool ExistsLocally ()
        {
            if (Parents.Count != 1)
                throw new ArgumentOutOfRangeException (String.Format ("Only if one parent exists, this method could return the corect answer, but there are {0} parents", Parents.Count));
            return File.Exists (Path.Combine (Parents [0].GetLocalPath (), Name));
        }

        public bool ExistsLocally (SyncFolder parent)
        {
            if (this.Parents.Contains (parent))
                return File.Exists (parent.GetLocalPath ());
            else 
                return false;
        }

        public string GetLocalPath (SyncFolder parent)
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
            if(this.LastLocalWriteTimeUtc == null) {
                return file.Exists;
            } else {
                if ( file.LastWriteTimeUtc.Equals (this.LastLocalWriteTimeUtc)) {
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

