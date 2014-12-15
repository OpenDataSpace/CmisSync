

namespace TestLibrary.TestUtils
{
    using System;

    using NUnit.Framework;

    [TestFixture]
    public class FolderTreeTest
    {
        [Test, Category("Fast")]
        public void ConstructFolderTree()
        {
            string tree = 
                ".\n" +
                    "├── A\n" +
                    "│   └── E\n" +
                    "│       ├── F\n" +
                    "│       └── G\n" +
                    "├── B\n" +
                    "└── C\n" +
                    "    └── D\n";
            var underTest = new FolderTree(tree);

            Console.WriteLine(underTest.ToString());

            Assert.That(underTest.ToString(), Is.EqualTo(tree));
        }
    }
}