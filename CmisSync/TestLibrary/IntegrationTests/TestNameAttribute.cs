
namespace TestLibrary.IntegrationTests
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class TestNameAttribute : Attribute
    {
        private string name;
        public TestNameAttribute(string name)
        {
            this.name = name;
        }

        public string Name {
            get {
                return this.name;
            }
        }
    }
}