
namespace CmisSync.Lib.Storage.Database {
    using System;
    using System.IO;

    public interface IDotTreeWriter<T> {
        void ToDotString(IObjectTree<T> tree, StreamWriter writer);
    }
}