
namespace TestLibrary.AlgorithmsTests
{
    using System;
    using System.Linq;

    using CmisSync.Lib.Algorithms;

    using NUnit.Framework;

    using TestLibrary.AlgorithmsTests;

    [TestFixture]
    public class TarjanSimpleLinearTests
    {
        /// <summary>
        /// A → B
        /// </summary>
        [Test, Category("Fast")]
        public void TwoNodes()
        {
            var a = new StringTarjanNode("A");
            var b = new StringTarjanNode("B");
            a.Neighbors.Add(b);

            var underTest = new Tarjan(a, b);

            Assert.That(underTest.ResultSets.Count, Is.EqualTo(2));
        }

        /// <summary>
        /// A → B
        /// C → D
        /// </summary>
        [Test, Category("Fast")]
        public void TwoIndepdendedChainsOfTwoNodes()
        {
            var a = new StringTarjanNode("A");
            var b = new StringTarjanNode("B");
            a.Neighbors.Add(b);

            var c = new StringTarjanNode("C");
            var d = new StringTarjanNode("D");
            c.Neighbors.Add(d);

            var underTest = new Tarjan(a, b, c, d);

            Assert.That(underTest.ResultSets.Count, Is.EqualTo(4));
        }

        /// <summary>
        /// A → B → C → D
        /// </summary>
        [Test, Category("Fast")]
        public void FourNodes()
        {
            var a = new StringTarjanNode("A");
            var b = new StringTarjanNode("B");
            var c = new StringTarjanNode("C");
            var d = new StringTarjanNode("D");
            a.Neighbors.Add(b);
            b.Neighbors.Add(c);
            c.Neighbors.Add(d);

            var underTest = new Tarjan(a, b, c, d);

            Assert.That(underTest.ResultSets.Count, Is.EqualTo(4));
        }

        /// <summary>
        /// A → B → C
        ///        ↳ D
        /// </summary>
        [Test, Category("Fast")]
        public void FourTreeNodes()
        {
            var a = new StringTarjanNode("A");
            var b = new StringTarjanNode("B");
            var c = new StringTarjanNode("C");
            var d = new StringTarjanNode("D");
            a.Neighbors.Add(b);
            b.Neighbors.Add(c);
            b.Neighbors.Add(d);

            var underTest = new Tarjan(a, b, c, d);

            Assert.That(underTest.ResultSets.Count, Is.EqualTo(4));
        }
    }
}