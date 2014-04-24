//-----------------------------------------------------------------------
// <copyright file="LocalSituationDetection.cs" company="GRAU DATA AG">
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
using System.IO;

using CmisSync.Lib.Storage;

using log4net;
using CmisSync.Lib.Events;

namespace CmisSync.Lib.Sync.Strategy
{
    public class LocalSituationDetection : ISituationDetection<AbstractFolderEvent>
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(LocalSituationDetection));

        public SituationType Analyse(IMetaDataStorage storage, AbstractFolderEvent actualEvent)
        {
            SituationType type = DoAnalyse(storage, actualEvent);
            logger.Debug(String.Format("Local Situation is: {0}", type));
            return type;
        }

        private SituationType DoAnalyse(IMetaDataStorage storage, AbstractFolderEvent actualEvent)
        {
            switch (actualEvent.Local) 
            {
                case MetaDataChangeType.CREATED:
                    return SituationType.ADDED;
                case MetaDataChangeType.DELETED:
                    return SituationType.REMOVED;
                case MetaDataChangeType.NONE:
                default:
                    return SituationType.NOCHANGE;
            }
        }
    }
}

