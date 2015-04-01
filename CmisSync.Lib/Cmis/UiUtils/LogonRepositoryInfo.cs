
namespace CmisSync.Lib.Cmis.UiUtils {
    using System;

    /// <summary>
    /// Logon repository info containing repo id and repo name.
    /// </summary>
    public class LogonRepositoryInfo {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Cmis.UiUtils.LogonRepositoryInfo"/> class.
        /// </summary>
        /// <param name="id">Repository Identifier.</param>
        /// <param name="name">Repository Name.</param>
        public LogonRepositoryInfo(string id, string name) {
            this.Id = id;
            this.Name = name;
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Cmis.UiUtils.LogonRepositoryInfo"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Cmis.UiUtils.LogonRepositoryInfo"/>.</returns>
        public override string ToString() {
            return string.Format("[LogonRepositoryInfo: Name={0}, Id={1}]", this.Name, this.Id);
        }
    }
}