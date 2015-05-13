//-----------------------------------------------------------------------
// <copyright file="Brand.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Config {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;

    /// <summary>
    /// Client Brand Configuration
    /// </summary>
    [Serializable]
    public class Brand {
        /// <summary>
        /// Gets or sets the CMIS server that holds the client brand files
        /// </summary>
        [XmlElement("server")]
        public XmlUri Server { get; set; }

        /// <summary>
        /// Gets or sets the client branding files
        /// </summary>
        [XmlArray("files")]
        [XmlArrayItem("file")]
        public List<BrandFile> Files { get; set; }
    }

    /// <summary>
    /// Client Brand file configuration
    /// </summary>
    [Serializable]
    public class BrandFile {
        /// <summary>
        /// pathname for the client brand file on CMIS repository
        /// </summary>
        [XmlElement("path")]
        public string Path { get; set; }

        /// <summary>
        /// Last Modification Date for the client brand file on CMIS repository 
        /// </summary>
        [XmlElement("date")]
        public DateTime Date { get; set; }
    }
}