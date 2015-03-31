//-----------------------------------------------------------------------
// <copyright file="ServerCredentials.cs" company="GRAU DATA AG">
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
    using System.Linq;
    using System.Text;

    using CmisSync.Lib.Cmis;

    /// <summary>
    /// Server Login for a specific Uri
    /// </summary>
    [Serializable]
    public class ServerCredentials : UserCredentials {
        /// <summary>
        /// Atom pub binding string.
        /// </summary>
        public static readonly string BindingAtomPub = DotCMIS.BindingType.AtomPub;

        /// <summary>
        /// Browser binding string.
        /// </summary>
        public static readonly string BindingBrowser = DotCMIS.BindingType.Browser;

        private string binding = BindingAtomPub;

        /// <summary>
        /// Gets or sets the server Address and Path
        /// </summary>
        public Uri Address { get; set; }

        /// <summary>
        /// Gets or sets the CMIS binding
        /// </summary>
        public string Binding {
            get {
                return this.binding;
            }

            set {
                this.binding = value;
            }
        }

        public override string ToString() {
            return string.Format("[ServerCredentials: Address={0}, Binding={1}, UserName={2}]", Address, Binding, UserName);
        }
    }
}