//-----------------------------------------------------------------------
// <copyright file="StartNextSyncEventTest.cs" company="GRAU DATA AG">
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
using System;

using CmisSync.Lib.Events;

using NUnit.Framework;

using Moq;
namespace TestLibrary.EventsTests
{
    [TestFixture]
    public class StartNextSyncEventTest
    {
        [Test, Category("Fast")]
        public void ContructorWithoutParamTest() {
            var start = new StartNextSyncEvent();
            Assert.IsFalse(start.FullSyncRequested);
        }

        [Test, Category("Fast")]
        public void ConstructorWithFalseParamTest() {
            var start = new StartNextSyncEvent(false);
            Assert.IsFalse(start.FullSyncRequested);
        }

        [Test, Category("Fast")]
        public void ConstructorWithTrueParamTest() {
            var start = new StartNextSyncEvent(true);
            Assert.IsTrue(start.FullSyncRequested);
        }

        [Test, Category("Fast")]
        public void ParamTest() {
            var start = new StartNextSyncEvent();
            string key = "key";
            string value = "value";

            start.SetParam(key, value);
            string result;
            Assert.IsTrue(start.TryGetParam(key, out result));
            Assert.AreEqual(value, result);
            Assert.IsFalse(start.TryGetParam("k", out result));
        }
    }
}

