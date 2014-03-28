#if __MonoCS__
using System;
using CmisSync.Lib.Storage;

using NUnit.Framework;

using Moq;

namespace TestLibrary
{
    [TestFixture]
    public class ExtendedAttributeReaderUnixTest
    {
        [Test, Category("Fast")]
        public void DefaultConstructorWorks()
        {
            new ExtendedAttributeReaderUnix();
        }
    }
}
#endif
