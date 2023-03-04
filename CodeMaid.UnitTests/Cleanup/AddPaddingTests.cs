using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public async Task ShouldPadPropertiesAsync()
        {
            var source =
@"
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
";

            var expected =
@"
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
";

            await testWorkspace.VerifyCleanupAsync(source, expected);
        }

        [TestMethod]
        public async Task ShouldPadMixed2Async()
        {
            var source =
@"
internal struct Struct
{
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

internal enum MyEnum
{
    Some = 0,
    None = 1,
}
";

            await testWorkspace.VerifyCleanupAsync(source, expected);
        }

        [TestMethod]
        public async Task ShouldPadSwitchCaseAsync()
        {
            var source =
@"
public class Class
{
    private void Do()
    {
        int number = 1;

        switch (number)
        {
            case 0:
                Console.WriteLine(""The number is zero"");
                break;

            case 1:
                Console.WriteLine(""The number is one"");
                break;

            case 2:
                Console.WriteLine(""The number is two"");
                break;

            default:
                Console.WriteLine(""The number is not zero, one, or two"");
                break;
        }
    }
}
";

            var expected =
@"
public class Class
{
    private void Do()
    {
        int number = 1;

        switch (number)
        {
            case 0:
                Console.WriteLine(""The number is zero"");
                break;

            case 1:
                Console.WriteLine(""The number is one"");
                break;

            case 2:
                Console.WriteLine(""The number is two"");
                break;

            default:
                Console.WriteLine(""The number is not zero, one, or two"");
                break;
        }
    }
}
";

            await testWorkspace.VerifyCleanupAsync(source, expected);
        }
    }
}