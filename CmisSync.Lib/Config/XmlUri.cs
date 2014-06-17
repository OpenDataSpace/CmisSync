//-----------------------------------------------------------------------
// <copyright file="XmlUri.cs" company="GRAU DATA AG">
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
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Xml Serializable URI.
    /// </summary>
    [Serializable]
    public class XmlUri : IXmlSerializable
    {
        private Uri uri;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Config.XmlUri"/> class.
        /// </summary>
        public XmlUri()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Config.XmlUri"/> class.
        /// </summary>
        /// <param name='source'>
        /// Source uri.
        /// </param>
        public XmlUri(Uri source)
        {
            this.uri = source;
        }

        /// <summary>
        /// Implicite conversion of a XmlUri into an Uri
        /// </summary>
        /// <param name='o'>
        /// Given XmlUri.
        /// </param>
        public static implicit operator Uri(XmlUri o)
        {
            return o == null ? null : o.uri;
        }

        /// <summary>
        /// Implicite conversion of an Uri into a XmlUri
        /// </summary>
        /// <param name='o'>
        /// Given original uri.
        /// </param>
        public static implicit operator XmlUri(Uri o)
        {
            return o == null ? null : new XmlUri(o);
        }

        /// <summary>
        /// Gets the schema.
        /// </summary>
        /// <returns>
        /// null schema
        /// </returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Reads the xml.
        /// </summary>
        /// <param name="reader">
        /// Xml Reader.
        /// </param>
        public void ReadXml(XmlReader reader)
        {
            this.uri = new Uri(reader.ReadElementContentAsString());
        }

        /// <summary>
        /// Writes the xml.
        /// </summary>
        /// <param name='writer'>
        /// Xml Writer.
        /// </param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteValue(this.uri.ToString());
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Config.XmlUri"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Config.XmlUri"/>.
        /// </returns>
        public override string ToString()
        {
            return this.uri.ToString();
        }
    }
}
