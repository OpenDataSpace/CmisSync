//-----------------------------------------------------------------------
// <copyright file="ObjectTreeTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.ProducerTests.CrawlerTests
{
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Data;

    using NUnit.Framework;

    [TestFixture]
    public class ObjectTreeTest
    {
        [Test, Category("Fast")]
        public void ConstructorSetsPropertiesToDefaultValuesAndNull()
        {
            var tree = new ObjectTree<object>();
            Assert.That(tree.Children, Is.Null);
            Assert.That(tree.Item, Is.Null);
        }

        [Test, Category("Fast")]
        public void ItemProperty()
        {
            var tree = new ObjectTree<object>();
            var obj = new object();
            tree.Item = obj;

            Assert.That(tree.Item, Is.EqualTo(obj));
        }

        [Test, Category("Fast")]
        public void ChildrenProperty()
        {
            var tree = new ObjectTree<object>();
            var obj = new object();
            IList<IObjectTree<object>> children = new List<IObjectTree<object>>();
            children.Add(new ObjectTree<object> {
                Item = obj
            });
            tree.Children = children;

            Assert.That(tree.Children, Is.Not.Null);
            Assert.That(tree.Children[0].Item, Is.EqualTo(obj));
        }

        [Test, Category("Fast")]
        public void ToListOfEmptyTreeReturnsEmptyList()
        {
            var tree = new ObjectTree<object>();
            Assert.That(tree.ToList(), Is.Empty);
        }

        [Test, Category("Fast")]
        public void ToListReturnsRootItemAsEntryInList()
        {
            var obj = new object();
            var tree = new ObjectTree<object> {
                Item = obj
            };

            Assert.That(tree.ToList(), Contains.Item(obj));
        }

        [Test, Category("Fast")]
        public void ToListReturnsChildrenAsEntryInList()
        {
            var obj = new object();
            IList<IObjectTree<object>> list = new List<IObjectTree<object>>();
            list.Add(new ObjectTree<object> {
                Item = obj
            });
            var tree = new ObjectTree<object> {
                Children = list
            };

            Assert.That(tree.ToList(), Contains.Item(obj));
        }
    }
}