using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace SteveCadwallader.CodeMaid.UnitTests.Cleanup
{
    [TestClass]
    public class AddPaddingTests
    {
        private readonly TestWorkspace testWorkspace;

        public AddPaddingTests()
        {
            testWorkspace = new TestWorkspace();
        }

        [TestMethod]
        public async Task ShouldPadClassesAsync()
        {
            var source =
@"
internal class MyClass
{
}
internal class MyClass2
{
}
";

            var expected =
@"
internal class MyClass
{
}

internal class MyClass2
{
}
";

            await testWorkspace.VerifyCleanupAsync(source, expected);
        }

        [TestMethod]
        public async Task ShouldPadMixed1Async()
        {
            var source =
@"
internal struct Struct
{
}
internal
    //
    class Temp
{
    public int MyProperty { get; set; }
    private void Do()
    {
    }
    private void Foo()
    {
    }
}
internal enum MyEnum
{
    Some = 0,
    None = 1,
}
";

            var expected =
@"
internal struct Struct
{
}

internal
    //
    class Temp
{
    public int MyProperty { get; set; }

    private void Do()
    {
    }

    private void Foo()
    {
    }
}

internal enum MyEnum
{
    Some = 0,
    None = 1,
}
";

            await testWorkspace.VerifyCleanupAsync(source, expected);
        }
    }
}