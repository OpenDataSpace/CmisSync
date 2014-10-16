//-----------------------------------------------------------------------
// <copyright file="FilterTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.SelectiveIgnoreTests
{
    using System;
    using System.Collections.ObjectModel;

    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.SelectiveIgnore;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class FilterTest
    {
        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorFailsIfQueueIsNull() {
            Assert.Throws<ArgumentNullException>(
                () => new SelectiveIgnoreFilter(
                null,
                new ObservableCollection<IIgnoredEntity>(),
                Mock.Of<ISession>()));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorFailsIfCollectionIsNull() {
            Assert.Throws<ArgumentNullException>(
                () => new SelectiveIgnoreFilter(
                Mock.Of<ISyncEventQueue>(),
                null,
                Mock.Of<ISession>()));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorFailsIfSessionIsNull() {
            Assert.Throws<ArgumentNullException>(
                () => new SelectiveIgnoreFilter(
                Mock.Of<ISyncEventQueue>(),
                new ObservableCollection<IIgnoredEntity>(),
                null));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorTest() {
            new SelectiveIgnoreFilter(
                Mock.Of<ISyncEventQueue>(),
                new ObservableCollection<IIgnoredEntity>(),
                Mock.Of<ISession>());
        }
    }
}