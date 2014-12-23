//-----------------------------------------------------------------------
// <copyright file="FileTransmissionStorage.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Storage.Database
{
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Storage.Database.Entities;

    using DBreeze;
    using DBreeze.DataTypes;

    public class FileTransmissionStorage : IFileTransmissionStorage
    {
        private static readonly string FileTransmissionObjectsTable = "FileTransmissionObjects";

        private DBreezeEngine Engine;

        static FileTransmissionStorage()
        {
            DBreezeInitializerSingleton.Init();
        }

        public FileTransmissionStorage(DBreezeEngine engine)
        {
            if (engine == null)
            {
                throw new ArgumentNullException("engine");
            }
            Engine = engine;
        }

        public IList<IFileTransmissionObject> GetObjectList()
        {
            List<IFileTransmissionObject> objects = new List<IFileTransmissionObject>();

            using (var tran = Engine.GetTransaction())
            {
                foreach (var row in tran.SelectForward<string, DbCustomSerializer<FileTransmissionObject>>(FileTransmissionObjectsTable))
                {
                    var value = row.Value;
                    if (value == null)
                    {
                        continue;
                    }

                    var data = value.Get;
                    if (data == null)
                    {
                        continue;
                    }

                    objects.Add(data);
                }
            }

            return objects;
        }

        public void SaveObject(IFileTransmissionObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            if (obj.LocalPath == null)
            {
                throw new ArgumentNullException("obj.LocalPath");
            }
            if (string.IsNullOrEmpty(obj.LocalPath))
            {
                throw new ArgumentException("empty string", "obj.LocalPath");
            }

            if (obj.RemoteObjectId == null)
            {
                throw new ArgumentNullException("obj.RemoteObjectId");
            }
            if (string.IsNullOrEmpty(obj.RemoteObjectId))
            {
                throw new ArgumentException("empty string", "obj.RemoteObjectId");
            }

            if (!(obj is FileTransmissionObject))
            {
                throw new ArgumentException("require FileTransmissionObject type", "obj");
            }

            using (var tran = Engine.GetTransaction())
            {
                tran.Insert<string, DbCustomSerializer<FileTransmissionObject>>(FileTransmissionObjectsTable, obj.RemoteObjectId, obj as FileTransmissionObject);
                tran.Commit();
            }
        }

        public IFileTransmissionObject GetObjectByRemoteObjectId(string remoteObjectId)
        {
            using (var tran = Engine.GetTransaction())
            {
                DbCustomSerializer<FileTransmissionObject> value = tran.Select<string, DbCustomSerializer<FileTransmissionObject>>(FileTransmissionObjectsTable, remoteObjectId).Value;
                if (value == null)
                {
                    return null;
                }
                return value.Get;
            }
        }

        public void RemoveObjectByRemoteObjectId(string remoteObjectId)
        {
            if (remoteObjectId == null)
            {
                throw new ArgumentNullException("remoteObjectId");
            }
            if (string.IsNullOrEmpty(remoteObjectId))
            {
                throw new ArgumentException("empty string", "remoteObjectId");
            }

            using (var tran = Engine.GetTransaction())
            {
                tran.RemoveKey(FileTransmissionObjectsTable, remoteObjectId);
                tran.Commit();
            }
        }

        public void ClearObjectList()
        {
            using (var tran = Engine.GetTransaction())
            {
                tran.RemoveAllKeys(FileTransmissionObjectsTable, true);
                tran.Commit();
            }
        }
    }
}
