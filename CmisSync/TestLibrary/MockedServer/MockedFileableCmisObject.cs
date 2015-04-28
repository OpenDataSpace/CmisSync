
namespace TestLibrary.MockedServer {
    using System;
    using System.Collections.Generic;

    using DotCMIS.Client;

    using Moq;

    public abstract class MockedFileableCmisObject<T> : MockedCmisObject<T> where T : class, IFileableCmisObject {
        public MockedFileableCmisObject(string name, string id = null, MockBehavior behavior = MockBehavior.Strict) : base(name, id, behavior) {
            this.Setup(m => m.Parents).Returns(() => new List<IFolder>(this.Parents));
            this.Setup(m => m.Move(It.IsAny<IObjectId>(), It.IsAny<IObjectId>())).Returns(this.Object);
            this.Setup(m => m.Paths).Returns(() => this.Paths);
        }

        public IList<IFolder> Parents { get; set; }

        public IList<string> Paths { get; set; }
    }
}