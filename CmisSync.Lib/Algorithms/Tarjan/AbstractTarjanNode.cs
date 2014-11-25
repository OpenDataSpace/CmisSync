
namespace CmisSync.Lib.Algorithms
{
    using System;
    using System.Collections.Generic;

    public abstract class AbstractTarjanNode
    {
        public AbstractTarjanNode(params AbstractTarjanNode[] neighbors) {
            this.Neighbors = new List<AbstractTarjanNode>(neighbors);
        }

        public long lowLink { get; set; }
        public long dfs { get; set; }
        public bool onStack { get; set; }
        public List<AbstractTarjanNode> Neighbors { get; private set; }
    }
}