using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteveCadwallader.CodeMaid.Properties;
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
internal class Temp
{
    public int MyProperty { get; set; }
    private void Do()
    {
    }
}
";

            var expected =
@"
internal class Temp
{
    public int MyProperty { get; set; }

    private void Do()
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

        [TestMethod]
        public async Task ShouldPadTypeAsync()
        {
            var source =
@"
class Class<T> where T : struct
{
}
";

            var expected =
@"
internal class Class<T> where T : struct
{
}
";

            await testWorkspace.VerifyCleanupAsync(source, expected);
        }

        [TestMethod]
        public async Task ShouldBetweenMultLineAccessorAsync()
        {
            var s = Settings.Default;
            var source =
@"
class Class
{
    public string FullName
    {
        get
        {
            return _fullName;
        }
        set => _fullName = value;
    }

    public string FirstName
    {
        get => _firstName;
        set => _firstName = value;
    }
}
";

            var expected =
@"
internal class Class
{
    public string FullName
    {
        get
        {
            return _fullName;
        }

        set => _fullName = value;
    }

    public string FirstName
    {
        get => _firstName;
        set => _firstName = value;
    }
}
";

            await testWorkspace.VerifyCleanupAsync(source, expected);
        }

        [TestMethod]
        public async Task ShouldPadCommentsAsync()
        {
            var source =
@"
internal
    //
    class Temp
{
    private void Do()
    {
    }
    // Single
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
    private void Do()
    {
    }

    // Single
    private void Foo()
    {
    }
}
";
            await testWorkspace.VerifyCleanupAsync(source, expected);
        }

    }
}