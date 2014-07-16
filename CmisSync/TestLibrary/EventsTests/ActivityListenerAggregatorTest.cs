//-----------------------------------------------------------------------
// <copyright file="ActivityListenerAggregatorTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary
{
    using System;

    using CmisSync.Lib;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;

    using NUnit.Framework;

    using Moq;

    [TestFixture]
    public class ActivityListenerAggregatorTest
    {
        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfTransmissionManagerIsNull() {
            new ActivityListenerAggregator(Mock.Of<IActivityListener>(), null);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfActivityListenerIsNull() {
            new ActivityListenerAggregator(null, new ActiveActivitiesManager());
        }

        [Test, Category("Fast")]
        public void ConstructorTakesTransmissionManager() {
            var manager = new ActiveActivitiesManager();
            var agg = new ActivityListenerAggregator(Mock.Of<IActivityListener>(), manager);
            Assert.That(agg.TransmissionManager, Is.EqualTo(manager));
        }
    }
}