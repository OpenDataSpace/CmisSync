
namespace CmisSync.Lib.Exceptions {
    using System;

    /// <summary>
    /// Exception level.
    /// </summary>
    public enum ExceptionLevel {
        /// <summary>
        /// The undecided. Should not occur.
        /// </summary>
        Undecided = 0,

        /// <summary>
        /// Information for the user
        /// </summary>
        Info,

        /// <summary>
        /// Warning about a problem
        /// </summary>
        Warning,

        /// <summary>
        /// Fatal warning
        /// </summary>
        Fatal
    }
}