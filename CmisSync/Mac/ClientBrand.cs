//-----------------------------------------------------------------------
// <copyright file="ClientBrand.cs" company="GRAU DATA AG">
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
﻿namespace CmisSync {
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib;

    public class ClientBrand : ClientBrandBase {
        public override List<string> PathList {
            get {
                List<string> pathList = new List<string>();
                pathList.Add("/DataSpaceSync/common/about.png");
                pathList.Add("/DataSpaceSync/common/about@2x.png");
                pathList.Add("/DataSpaceSync/common/side-splash.png");
                pathList.Add("/DataSpaceSync/common/side-splash@2x.png");
                pathList.Add("/DataSpaceSync/mac/folder.icns");
                return pathList;
            }
        }
    }
}