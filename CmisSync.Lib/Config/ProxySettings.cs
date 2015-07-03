//-----------------------------------------------------------------------
// <copyright file="ProxySettings.cs" company="GRAU DATA AG">
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
    using System.ComponentModel;
    using System.Xml.Serialization;
    
    /// <summary>
    /// Proxy selection method.
    /// </summary>
    [Serializable]
    public enum ProxySelection {
        /// <summary>
        /// Use the system settings.
        /// </summary>
        SYSTEM,

        /// <summary>
        /// Only connect without proxy.
        /// </summary>
        NOPROXY,

        /// <summary>
        /// Use custom proxy settings.
        /// </summary>
        CUSTOM
    }

    /// <summary>
    /// Proxy settings struct.
    /// </summary>
    [Serializable]
    public struct ProxySettings {
        /// <summary>
        /// Gets or sets the way if and how to use the systems proxy settings.
        /// </summary>
        /// <value>
        /// The selection.
        /// </value>
        [XmlAttribute("selected")]
        [DefaultValue(ProxySelection.SYSTEM)]
        public ProxySelection Selection { get; set; }

        /// <summary>
        /// Gets or sets the server url.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        [XmlElement("server")]
        [DefaultValue(null)]
        public XmlUri Server { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CmisSync.Lib.Config.ProxySettings"/> login required.
        /// </summary>
        /// <value>
        /// <c>true</c> if login required; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("loginRequired")]
        [DefaultValue(false)]
        public bool LoginRequired { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        [XmlElement("username")]
        [DefaultValue(null)]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the obfuscated password.
        /// </summary>
        /// <value>
        /// The obfuscated password.
        /// </value>
        [XmlElement("password")]
        [DefaultValue(null)]
        public string ObfuscatedPassword { get; set; }
    }
}