
using System.Runtime.CompilerServices;

namespace SrcGen.Generated;

public static class CallerTypeNameExtensions
{

    public static void DoSomethingCallerTypeName([CallerMemberName] string callerMemberName = "")
    {
        SrcGen.Demo.MyClass.DoSomething();
        System.Console.WriteLine($"Called from: {callerMemberName} in {typeof(SrcGen.Demo.MyClass).Name}");
    }

}
