using System;
using System.IO;

using DotCMIS.Client;

namespace CmisSync.Lib.Events
{
    public class FileDownloadRequest : ISyncEvent
    {
        private IDocument doc;
        public IDocument Document { get{return this.doc;} }
        private string localPath;
        public string LocalPath { get{ return this.localPath;} }
        public string TargetFilePath { get{ return Path.Combine(localPath, this.doc.Name);} }
        public FileDownloadRequest (IDocument doc, string localPath)
        {
            if(doc == null)
                throw new ArgumentNullException("The document object which should be downloaded must not be null");
            if(localPath == null)
                throw new ArgumentNullException(String.Format("The target directory path where the document \"{0}\" should be saved cannot be null", doc.Name));
            this.doc = doc;
            this.localPath = localPath;
        }

        public override bool Equals (object obj)
        {
            FileDownloadRequest other = obj as FileDownloadRequest;
            if(other == null)
                return false;
            if(other.Document.Equals(this.doc) && other.LocalPath.Equals(this.LocalPath))
                return true;
            else
                return false;
        }

        public override int GetHashCode ()
        {
            return base.GetHashCode ();
        }

        public override string ToString() {
            return String.Format("FileDownloadRequest: targetFilePath=\"{0}\"", Path.Combine(localPath, doc.Name));
        }
    }
}

