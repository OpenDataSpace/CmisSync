//-----------------------------------------------------------------------
// <copyright file="IMappedObject.cs" company="GRAU DATA AG">
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
using System;
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Collections.Generic;

using CmisSync.Lib;
using CmisSync.Lib.Storage;

using DotCMIS.Client;

namespace CmisSync.Lib.Data
{
    public interface IMappedObject
    {
        string RemoteObjectId { get; set; }

        string ParentId { get; set; }

        string LastChangeToken { get; set; }

        DateTime? LastRemoteWriteTimeUtc { get; set; }

        DateTime? LastLocalWriteTimeUtc { get; set; }

        byte[] LastChecksum { get; set; }

        string ChecksumAlgorithmName { get; set; }

        string Name { get; set; }

        string Description { get; set; }

        Guid Guid { get; set; }

        MappedObjectType Type { get; }
    }

}
