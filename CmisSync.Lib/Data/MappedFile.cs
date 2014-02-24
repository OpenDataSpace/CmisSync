using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;

using CmisSync.Lib.Storage;

using DotCMIS.Client;

namespace CmisSync.Lib.Data
{

    [Serializable]
    public class MappedFile : AbstractMappedObject
    {
        public List<MappedFolder> Parents { get; set; }

        [DefaultValue(-1)]
        public long LastFileSize { get; set; }

        public MappedFile (MappedFolder parent, IFileSystemInfoFactory fsFactory = null, params MappedFolder[] parents)
            : base(parent.LocalSyncTargetPath, parent.RemoteSyncTargetPath, fsFactory)
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
            return FsFactory.CreateFileInfo(Path.Combine (Parents [0].GetLocalPath (), Name)).Exists;
        }

        public bool ExistsLocally (MappedFolder parent)
        {
            if (this.Parents.Contains (parent))
                return FsFactory.CreateFileInfo(parent.GetLocalPath ()).Exists;
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
            IFileInfo file = FsFactory.CreateFileInfo(GetLocalPath ());
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

