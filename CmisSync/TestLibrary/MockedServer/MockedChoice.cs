//-----------------------------------------------------------------------
// <copyright file="MockedChoice.cs" company="GRAU DATA AG">
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

namespace TestLibrary.MockedServer {
    using System;
    using System.Collections.Generic;

    using DotCMIS.Data;

    using Moq;

    public class MockedChoice<T> : Mock<IChoice<T>> {
        public MockedChoice(T defaultValue, MockBehavior behavior = MockBehavior.Strict, params IChoice<T>[] choices) : base(behavior) {
            this.Choices = new List<IChoice<T>>(choices);
            this.Setup(m => m.DisplayName).Returns(() => this.DisplayName);
            this.Setup(m => m.Choices).Returns(() => new List<IChoice<T>>(this.Choices));
        }

        public IList<T> Value { get; set; }

        public IList<IChoice<T>> Choices { get; set; }

        public string DisplayName { get; set; }
    }
}