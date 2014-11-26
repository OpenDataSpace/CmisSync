//-----------------------------------------------------------------------
// <copyright file="Tarjan.cs" company="GRAU DATA AG">
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