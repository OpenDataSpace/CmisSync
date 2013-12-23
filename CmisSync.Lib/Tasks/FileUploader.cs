using System;
using DotCMIS.Client;
using CmisSync.Lib.Events;
namespace CmisSync.Lib
{
	namespace Tasks {
		public abstract class FileUploader
		{
			private string localpath;
			private IFolder remoteFolder;
			private FileTransmissionEvent Status;
			public FileUploader(string localpath, IFolder remoteFolder, FileTransmissionEvent TransmissionStatus){
				if(String.IsNullOrEmpty(localpath))
					throw new ArgumentException("Could not upload an empty local filepath");
				if(remoteFolder == null)
					throw new ArgumentNullException("Could not upload to a non existing IFolder instance");
				Status = TransmissionStatus;
			}
		}
	}
}

