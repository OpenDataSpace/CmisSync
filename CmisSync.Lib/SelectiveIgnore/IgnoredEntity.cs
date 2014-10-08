
namespace CmisSync.Lib.SelectiveIgnore
{
    using System;

    using CmisSync.Lib.PathMatcher;

    using DotCMIS.Client;

    public class IgnoredEntity
    {
        public IgnoredEntity(IFolder folder, IPathMatcher matcher) {
            if (folder == null) {
                throw new ArgumentNullException("Given folder is null");
            }

            if (matcher == null) {
                throw new ArgumentNullException("Given matcher is null");
            }

            if (!matcher.CanCreateLocalPath(folder)) {
                throw new ArgumentException("Cannot create a local path for the given remote folder");
            }

            this.ObjectId = folder.Id;
            this.LocalPath = matcher.CreateLocalPath(folder);
        }

        public string ObjectId { get; private set; }
        public string LocalPath { get; private set; }
    }
}