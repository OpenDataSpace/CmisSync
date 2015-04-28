
namespace TestLibrary.MockedServer {
    using System;

    using DotCMIS.Data;

    using Moq;

    public class MockedAce : Mock<IAce> {
        public MockedAce(string principal, bool isDirect = true, MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.IsDirect = isDirect;
            this.Principal = new MockedPrincipal(behavior) { Id = principal }.Object;
            this.Setup(m => m.IsDirect).Returns(() => this.IsDirect);
            this.Setup(m => m.Principal).Returns(() => this.Principal);
        }

        public bool IsDirect { get; set; }

        public IPrincipal Principal { get; set; }
    }
}