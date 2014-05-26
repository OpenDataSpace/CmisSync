

namespace CmisSync.Lib
{
    using System;
    using System.Collections.Generic;

    public interface IObjectTree<T>
    {
        T Item { get; set; }
        int Flag { get; set; }
        IList<IObjectTree<T>> Children { get; set; }
    }
}

