

namespace TestLibrary.MockedServer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Enums;
    using Moq;
    using TestLibrary.TestUtils;

    public class MockedFolder : Mock<IFolder>
    {
        public MockedFolder(string name, IFolder parent = null) : base(MockBehavior.Strict) {

        }
    }
}