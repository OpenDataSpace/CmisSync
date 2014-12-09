
namespace CmisSync.Lib.Consumer
{
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Exceptions;

    /// <summary>
    /// Interaction needed exception. This exception should be thrown if a user must be informed about a needed interaction to solve a problem/conflict.
    /// </summary>
    public class InteractionNeededException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.InteractionNeededException"/> class.
        /// </summary>
        public InteractionNeededException() : base() {
            this.InitParams();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.InteractionNeededException"/> class.
        /// </summary>
        /// <param name="msg">Exception message.</param>
        public InteractionNeededException(string msg) : base(msg) {
            this.InitParams();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.InteractionNeededException"/> class.
        /// </summary>
        /// <param name="msg">Exception message.</param>
        /// <param name="inner">Inner exception.</param>
        public InteractionNeededException(string msg, Exception inner) : base(msg, inner) {
            this.InitParams();
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the details. The details are technical details, such as error messages from server or a stack trace.
        /// </summary>
        /// <value>The details of the problem.</value>
        public string Details { get; set; }

        /// <summary>
        /// Gets the actions, which can be executed to solve the problem.
        /// </summary>
        /// <value>The actions.</value>
        public Dictionary<string, Action> Actions { get; private set; }

        /// <summary>
        /// Gets the affected files. If local files or folders are involved to the problem, they should be listed.
        /// </summary>
        /// <value>The affected files.</value>
        public List<IFileSystemInfo> AffectedFiles { get; private set; }

        private void InitParams() {
            this.AffectedFiles = new List<IFileSystemInfo>();
            this.Actions = new Dictionary<string, Action>();
            this.Title = this.GetType().Name;
            this.Description = this.Message;
            this.Details = string.Empty;
            if (this.InnerException is CmisBaseException) {
                this.Details = (this.InnerException as CmisBaseException).ErrorContent;
            } else if (this.InnerException != null) {
                this.Details = this.InnerException.StackTrace ?? string.Empty;
            }
        }
    }
}