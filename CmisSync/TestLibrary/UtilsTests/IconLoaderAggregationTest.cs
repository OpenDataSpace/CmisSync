//-----------------------------------------------------------------------
// <copyright file="IconLoaderAggregationTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.UtilsTests {
    using System;

    using CmisSync.Lib.UiUtils;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class IconLoaderAggregationTest {
        [Test, Category("Fast")]
        public void AggregationConstructorFailsOnPassingNullAsIconLoaderInstance() {
            Assert.Throws<ArgumentNullException>(() => new AggregatingIconLoader(null, Mock.Of<IIconLoader>()));
        }

        [Test, Category("Fast")]
        public void AggregationConstructorFailsOnPassingNullAsFallbackIconLoaderInstance() {
            Assert.Throws<ArgumentNullException>(() => new AggregatingIconLoader(Mock.Of<IIconLoader>(), null));
        }

        [Test, Category("Fast")]
        public void AggregationConstructorTakesTwoInstances() {
            new AggregatingIconLoader(Mock.Of<IIconLoader>(), Mock.Of<IIconLoader>());
        }

        [Test, Category("Fast")]
        public void AggregationConstructorTakesMoreThanTwoInstances([Values(3, 4, 20)]int count) {
            var firstLoader = Mock.Of<IIconLoader>();
            var secondLoader = Mock.Of<IIconLoader>();
            var moreLoader = new IIconLoader[count - 2];
            for (int i = 0; i < count - 2; i++) {
                moreLoader[i] = Mock.Of<IIconLoader>();
            }

            new AggregatingIconLoader(firstLoader, secondLoader, moreLoader);
        }

        [Test, Category("Fast")]
        public void IfFirstLoaderReturnsAValueTheFallbackIsIgnored() {
            var path = "path" + Guid.NewGuid().ToString();
            var icon = Icons.ApplicationIcon;
            var first = new Mock<IIconLoader>(MockBehavior.Strict);
            first.Setup(l => l.GetPathOf(icon)).Returns(path);
            var second = new Mock<IIconLoader>(MockBehavior.Strict).Object;

            var underTest = new AggregatingIconLoader(first.Object, second);

            Assert.That(underTest.GetPathOf(icon), Is.EqualTo(path));
        }

        [Test, Category("Fast")]
        public void FallbackIsUsedIfFirstLoaderReturnsNull() {
            var path = "fallback" + Guid.NewGuid().ToString();
            var icon = Icons.ApplicationIcon;
            var first = new Mock<IIconLoader>();
            var fallback = new Mock<IIconLoader>();
            fallback.Setup(f => f.GetPathOf(icon)).Returns(path);

            var underTest = new AggregatingIconLoader(first.Object, fallback.Object);

            Assert.That(underTest.GetPathOf(icon), Is.EqualTo(path));
            first.Verify(f => f.GetPathOf(icon), Times.Once());
            fallback.Verify(f => f.GetPathOf(icon), Times.Once());
        }

        [Test, Category("Fast")]
        public void AllFallbacksAreCalledUntilValueIsReturned() {
            var path = "fallback" + Guid.NewGuid().ToString();
            var icon = Icons.ApplicationIcon;
            var firstLoader = Mock.Of<IIconLoader>();
            var secondLoader = Mock.Of<IIconLoader>();
            var thirdLoader = Mock.Of<IIconLoader>((l) => l.GetPathOf(icon) == path);
            var lastLoader = new Mock<IIconLoader>(MockBehavior.Strict).Object;

            var underTest = new AggregatingIconLoader(firstLoader, secondLoader, thirdLoader, lastLoader);

            Assert.That(underTest.GetPathOf(icon), Is.EqualTo(path));
        }
    }
}