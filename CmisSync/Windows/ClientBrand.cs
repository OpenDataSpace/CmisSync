//-----------------------------------------------------------------------
// <copyright file="ClientBrandBase.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib;

    class ClientBrand : ClientBrandBase {
        public override List<string> PathList {
            get {
                List<string> pathList = new List<string>();
                pathList.Add("/branding/DataSpaceSync/common/about.png");
                pathList.Add("/branding/DataSpaceSync/common/about@2x.png");
                pathList.Add("/branding/DataSpaceSync/common/side-splash.png");
                pathList.Add("/branding/DataSpaceSync/common/side-splash@2x.png");
                pathList.Add("/branding/DataSpaceSync/windows/app.ico");
                pathList.Add("/branding/DataSpaceSync/windows/folder.ico");
                return pathList;
            }
        }
    }
}