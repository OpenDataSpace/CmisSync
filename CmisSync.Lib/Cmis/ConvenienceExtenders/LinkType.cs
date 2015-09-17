
namespace CmisSync.Lib.Cmis.ConvenienceExtenders {
    using System;

    using DotCMIS.Enums;

    /// <summary>
    /// Possible GDS link types.
    /// </summary>
    public enum LinkType {
        /// <summary>
        /// The link type is unknown.
        /// </summary>
        [CmisValue("UNDEFINED")]
        Unknown,

        /// <summary>
        /// Upload link.
        /// </summary>
        [CmisValue("gds:uploadLink")]
        UploadLink,

        /// <summary>
        /// Download link.
        /// </summary>
        [CmisValue("gds:downloadLink")]
        DownloadLink
    }
}