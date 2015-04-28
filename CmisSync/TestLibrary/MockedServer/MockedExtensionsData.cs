

namespace TestLibrary.MockedServer {
    using System;
    using System.Collections.Generic;

    using DotCMIS.Data.Extensions;

    using Moq;

    public class MockedExtensionsData : Mock<IExtensionsData> {
        public MockedExtensionsData(MockBehavior behavior = MockBehavior.Strict) {
            this.Setup(m => m.Extensions).Returns(() => new List<ICmisExtensionElement>(this.Extensions));
        }

        public List<ICmisExtensionElement> Extensions { get; set; }
    }
}