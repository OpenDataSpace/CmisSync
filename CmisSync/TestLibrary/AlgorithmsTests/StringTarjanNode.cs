
namespace TestLibrary.AlgorithmsTests
{
    using System;

    using CmisSync.Lib.Algorithms;

    public class StringTarjanNode : AbstractTarjanNode {
        private string Name;
        public StringTarjanNode(string name, params AbstractTarjanNode[] neighbors) : base(neighbors) {
            this.Name = name;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}