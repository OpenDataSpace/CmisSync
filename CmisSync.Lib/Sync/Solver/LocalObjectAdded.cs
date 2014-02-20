using System;
using System.IO;

using DotCMIS.Client;

using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Sync.Solver
{
    public class LocalObjectAdded : ISolver
    {
        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, string remoteId){
            // Upload new file
            if((localFile.Attributes & FileAttributes.Directory) == FileAttributes.Directory) {
                // Create remote folder
                string remotePath = storage.Matcher.CreateRemotePath(new DirectoryInfo(localFile.FullName));
                //session.CreateFolder();
            }else if((localFile.Attributes & FileAttributes.Normal) == FileAttributes.Normal) {
                // Create empty remote file
                string remotePath = storage.Matcher.CreateRemotePath(localFile.FullName);
                //session.CreateDocument();
            }
            throw new NotImplementedException();
        }
    }
}

