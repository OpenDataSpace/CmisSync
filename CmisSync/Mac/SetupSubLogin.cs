//-----------------------------------------------------------------------
// <copyright file="SetupSubLogin.cs" company="GRAU DATA AG">
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

namespace CmisSync {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using MonoMac.AppKit;
    using MonoMac.Foundation;
    public partial class SetupSubLogin : MonoMac.AppKit.NSView {
        #region Constructors

        // Called when created from unmanaged code
        public SetupSubLogin(IntPtr handle) : base(handle) {
            this.Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public SetupSubLogin(NSCoder coder) : base(coder) {
            this.Initialize();
        }

        // Shared initialization code
        void Initialize() {
        }

        #endregion
    }
}