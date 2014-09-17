//-----------------------------------------------------------------------
// <copyright file="Feature.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------

namespace CmisSync.Lib.Config
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Feature Toggles.
    /// </summary>
    [Serializable]
    public class Feature
    {
        /// <summary>
        /// Gets or sets the getFolderTree support.
        /// </summary>
        /// <value>The getFolderTree support.</value>
        [XmlElement("getFolderTree")]
        public bool? GetFolderTreeSupport { get; set; }

        /// <summary>
        /// Gets or sets the getDescendants support.
        /// </summary>
        /// <value>The getDescendants support.</value>
        [XmlElement("getDescendants")]
        public bool? GetDescendantsSupport { get; set; }

        /// <summary>
        /// Gets or sets the getContentChanges support.
        /// </summary>
        /// <value>The getContentChanges support.</value>
        [XmlElement("getContentChanges")]
        public bool? GetContentChangesSupport { get; set; }

        /// <summary>
        /// Gets or sets the fileSystemWatcher support.
        /// </summary>
        /// <value>The fileSystemWatcher support.</value>
        [XmlElement("fileSystemWatcher")]
        public bool? FileSystemWatcherSupport { get; set; }

        /// <summary>
        /// Gets or sets the max number of content changes.
        /// </summary>
        /// <value>The max number of content changes.</value>
        [XmlElement("maxContentChanges")]
        public int? MaxNumberOfContentChanges { get; set; }

        /// <summary>
        /// Gets or sets the chunked support.
        /// </summary>
        /// <value>The chunked support.</value>
        [XmlElement("chunkedSupport")]
        public bool? ChunkedSupport { get; set; }

        /// <summary>
        /// Gets or sets the chunked download support.
        /// </summary>
        /// <value>The chunked download support.</value>
        [XmlElement("chunkedDownloadSupport")]
        public bool? ChunkedDownloadSupport { get; set; }
    }
}
