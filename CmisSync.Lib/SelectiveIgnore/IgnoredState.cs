namespace CmisSync.Lib.SelectiveIgnore
{
    using System;

    /// <summary>
    /// Ignored state of an object.
    /// </summary>
    public enum IgnoredState
    {
        /// <summary>
        /// This object is not ignored.
        /// </summary>
        NOT_IGNORED,

        /// <summary>
        /// This object is ignored.
        /// </summary>
        IGNORED,

        /// <summary>
        /// This object is a sub/child object of an ignored object
        /// </summary>
        INHERITED
    }
}