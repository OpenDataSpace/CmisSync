//-----------------------------------------------------------------------
// <copyright file="TarjanSimpleLinearTests.cs" company="GRAU DATA AG">
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