using System.Collections.Generic;


namespace TestLibrary.MockedServer {
    using System;

    using DotCMIS.Data;

    using Moq;

    public class MockedAcl : Mock<IAcl> {
        private List<IAce> Aces = new List<IAce>();
        public MockedAcl(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.Setup(m => m.Aces).Returns(() => new List<IAce>(this.Aces));
            this.Setup(m => m.IsExact).Returns(() => this.IsExact);
        }

        public bool? IsExact { get; set; }
    }
}