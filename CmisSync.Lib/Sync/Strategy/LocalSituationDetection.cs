using System;
using System.IO;

using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Sync.Strategy
{
    public class LocalSituationDetection : ISituationDetection<FileSystemInfo>
    {
        public SituationType Analyse(MetaDataStorage storage, FileSystemInfo actualObject)
        {
            throw new NotImplementedException();
        }
    }
}

