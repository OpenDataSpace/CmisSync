
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