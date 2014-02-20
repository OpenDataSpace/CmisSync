using System;
using System.IO;

using CmisSync.Lib.Events;
using CmisSync.Lib.Data;
using CmisSync.Lib.Storage;

using DotCMIS.Client;
using DotCMIS.Exceptions;

namespace CmisSync.Lib.Sync.Strategy
{
    public class FolderSynchronizer : ReportingSyncEventHandler
    {
        public static readonly int FOLDER_SYNCHRONIZER_PRIORITY = 0;
        private MetaDataStorage MetaData;
        private ISession Session;

        private IFileSystemInfoFactory fsFactory;

        public FolderSynchronizer (ISyncEventQueue queue, MetaDataStorage metadata, ISession session, FileSystemInfoFactory fsFactory = null) : base (queue)
        {
            if (metadata == null)
                throw new ArgumentNullException ("Given Metadata is null");
            if (session == null)
                throw new ArgumentNullException ("Given Session is null");
            this.MetaData = metadata;
            this.Session = session;

            if(fsFactory == null){
                this.fsFactory = new FileSystemInfoFactory();
            }else{
                this.fsFactory = fsFactory;
            }
        }

        public override int Priority {
            get {
                return FOLDER_SYNCHRONIZER_PRIORITY;
            }
        }

        public override bool Handle (ISyncEvent e)
        {
            FolderEvent folderEvent = e as FolderEvent;
            if (folderEvent != null) {
                SyncFolder (folderEvent);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Download a single folder from the CMIS server for sync.
        /// </summary>
        private void SyncFolder (FolderEvent folder)
        {
            if (folder.LocalFolder == null && folder.RemoteFolder == null)
                throw new ArgumentNullException ("Local and remote folder are null");


            bool? isLocallyAvailable = null;
            bool? isRemotelyAvailable = null;
            bool? wasAvailable = null;

            if(folder.LocalFolder != null && folder.RemoteFolder != null)


            if (folder.RemoteFolder == null) {
                // Could happen, if the local fs throws an event, or the IFolder has been removed on server.
                folder.LocalFolder.Refresh ();
                isLocallyAvailable = folder.LocalFolder.Exists;
                MappedFolder savedFolder;
                if( MetaData.TryGetMappedFolder(folder.LocalFolder, out savedFolder) ) {
                    wasAvailable = true;
                    try{
                        folder.RemoteFolder = Session.GetObject(savedFolder.RemoteObjectId) as IFolder;
                        isRemotelyAvailable = true;
                    }catch(CmisObjectNotFoundException) {
                        isRemotelyAvailable = false;
                    }
                } else {
                    wasAvailable = false;
                }
            }

            if (folder.LocalFolder == null) {
                try {
                    // Refresh object or figure out, if it is available at the moment
                    folder.RemoteFolder = Session.GetObject (folder.RemoteFolder.Id) as IFolder;
                    isRemotelyAvailable = true;

                    // Check, if the remote object has been synced in the past
                    wasAvailable = MetaData.GetFolderPath (folder.RemoteFolder.Id) != null;
                } catch (CmisObjectNotFoundException) {
                    isRemotelyAvailable = false;
                }
            }


            if (folder.LocalFolder == null) {
                // Try to figure out, if remoteFolder does exists remotely
                bool remoteHasBeenDeleted = false;
                try {
                    // Refresh object or figure out, if it is available any more
                    folder.RemoteFolder = Session.GetObject (folder.RemoteFolder.Id) as IFolder;
                } catch (CmisObjectNotFoundException) {
                    remoteHasBeenDeleted = true;
                }

                // Try to figure out the local folder path
                string lastLocalSavedPath = MetaData.GetFolderPath (folder.RemoteFolder.Id);
                if (lastLocalSavedPath == null) {
                    // There hasn't been any folder saved with the given remote folder id
                    if (remoteHasBeenDeleted)
                        // Nothing to do, because remote folder Id is not available locally and remotely
                        return;
                    // Try to figure out, if the correct path is available
                    string newLocalPath = MetaData.CreatePathFromRemoteFolder (folder.RemoteFolder);
                    if (MetaData.ContainsFolder (newLocalPath)) {
                        if (Directory.Exists (newLocalPath)) {
                            // Merge / Update the path
                            throw new NotImplementedException ();
                        } else {
                            // 
                        }
                    } else {
                        var dirInfo = fsFactory.CreateDirectoryInfo(newLocalPath);
                        dirInfo.Create();
                        MetaData.AddFolder (dirInfo, folder.RemoteFolder);
                    }
                } else {
                    // Do the path matches ? If not, a movement has been done, otherwise, do nothing
                    throw new NotImplementedException ();
                }
                throw new NotImplementedException ();
            } else if (folder.RemoteFolder == null) {
                throw new NotImplementedException ();
            } else {
                throw new NotImplementedException ();
            }

/*
                string name = remoteSubFolder.Name;
                string remotePathname = remoteSubFolder.Path;
                string localSubFolder = Path.Combine(localFolder, name);
                if(!Directory.Exists(localFolder))
                {
                    // The target folder has been removed/renamed => relaunch sync
                    Logger.Warn("The target folder has been removed/renamed: "+ localFolder);
                    return false;
                }

                if (Directory.Exists(localSubFolder))
                {
                    return true;
                }

                if (database.ContainsFolder(localSubFolder))
                {
                    // If there was previously a folder with this name, it means that
                    // the user has deleted it voluntarily, so delete it from server too.

                    // Delete the folder from the remote server.
                    Logger.Debug(String.Format("CMIS::DeleteTree({0})",remoteSubFolder.Path));
                    try{
                        remoteSubFolder.DeleteTree(true, null, true);
                        // Delete the folder from database.
                        database.RemoveFolder(localSubFolder);
                    }catch(Exception)
                    {
                        Logger.Info("Remote Folder could not be deleted: "+ remoteSubFolder.Path);
                        // Just go on and try it the next time
                    }
                }
                else
                {
                    // The folder has been recently created on server, so download it.

                    // If there was previously a file with this name, delete it.
                    // TODO warn if local changes in the file.
                    if (File.Exists(localSubFolder))
                    {
                        Logger.Warn("Local file \"" + localSubFolder + "\" has been renamed to \"" + localSubFolder + ".conflict\"");
                        File.Move(localSubFolder, localSubFolder + ".conflict");
                        this.Queue.AddEvent(new FileConflictEvent(FileConflictType.REMOTE_ADDED_PATH_CONFLICTS_LOCAL_FILE, localFolder, localSubFolder + ".conflict"));
                    }

                    // Skip if invalid folder name. See https://github.com/nicolas-raoul/CmisSync/issues/196
                    if (Utils.IsInvalidFolderName(name))
                    {
                        Logger.Info("Skipping download of folder with illegal name: " + name);
                    }
                    else if (repoinfo.isPathIgnored(remotePathname))
                    {
                        Logger.Info("Skipping dowload of ignored folder: " + remotePathname);
                    }
                    else
                    {
                        // Create local folder.remoteDocument.Name
                        Logger.Info("Creating local directory: " + localSubFolder);
                        Directory.CreateDirectory(localSubFolder);

                        // Create database entry for this folder.
                        // TODO - Yannick - Add metadata
                        database.AddFolder(localSubFolder, remoteSubFolder.Id, remoteSubFolder.LastModificationDate);
                    }
                }

                return true;*/
        }

    }
}

