
namespace CmisSync.Lib.Algorithms
{
    using System;
    using System.Collections.Generic;

    public class Tarjan
    {
        private Stack<AbstractTarjanNode> stack = new Stack<AbstractTarjanNode>();
        private HashSet<AbstractTarjanNode> nodes;
        private long maxdfs = 0;

        public Tarjan(params AbstractTarjanNode[] nodes)
        {
            this.ResultSets = new List<List<AbstractTarjanNode>>();
            this.nodes = new HashSet<AbstractTarjanNode>(nodes);
            while (this.nodes.Count > 0) {
                var enumerator = this.nodes.GetEnumerator();
                enumerator.MoveNext();
                this.Run(enumerator.Current);
            }
        }

        private void Run(AbstractTarjanNode node) {
            node.dfs = this.maxdfs;
            node.lowLink = this.maxdfs;
            this.maxdfs++;
            this.stack.Push(node);
            node.onStack = true;
            this.nodes.Remove(node);
            foreach (var neighbor in node.Neighbors) {
                if (this.nodes.Contains(neighbor)) {
                    this.Run(neighbor);
                    node.lowLink = Math.Min(node.lowLink, neighbor.lowLink);
                } else if (neighbor.onStack) {
                    node.lowLink = Math.Min(node.lowLink, neighbor.dfs);
                }
            }

            if (node.lowLink == node.dfs) {
                var s = new List<AbstractTarjanNode>();
                this.ResultSets.Add(s);
                AbstractTarjanNode n;
                do {
                    n = this.stack.Pop();
                    n.onStack = false;
                    s.Add(n);
                } while (node != n);
            }
        }

        public List<List<AbstractTarjanNode>> ResultSets { get; private set; }
    }
}