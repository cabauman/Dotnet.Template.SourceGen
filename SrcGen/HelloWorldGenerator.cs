using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace SrcGen;

[Generator]
public class HelloWorldGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all method symbols with the MySpecialAttribute
        var methodsWithAttribute = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "SrcGen.MySpecialAttribute",
                predicate: static (node, _) => node is MethodDeclarationSyntax,
                transform: static (ctx, _) => (IMethodSymbol)ctx.TargetSymbol
            )
            .Where(static m => m is not null);

        // Combine with the compilation and collect the results
        IncrementalValueProvider<(Compilation, ImmutableArray<IMethodSymbol>)> compilationAndMethods
            = context.CompilationProvider.Combine(methodsWithAttribute.Collect());

        // Generate source for each method
        context.RegisterSourceOutput(compilationAndMethods,
            static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    static void Execute(Compilation compilation, ImmutableArray<IMethodSymbol> methods, SourceProductionContext context)
    {
        if (methods.IsDefaultOrEmpty)
        {
            // nothing to do yet
            return;
        }

        // I'm not sure if this is even necessary, but `[LoggerMessage]` does it, so seems like a good idea!
        List<IMethodSymbol> methodsToGenerate = new List<IMethodSymbol>();
        foreach (IMethodSymbol method in methods)
        {
            if (method is IMethodSymbol)
                methodsToGenerate.Add(method);
        }

        if (methodsToGenerate.Count > 0)
            GenerateCallerTypeNameExtensions(context, methodsToGenerate);
    }

    static void GenerateCallerTypeNameExtensions(SourceProductionContext context, List<IMethodSymbol> methodsToGenerate)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append(@"
using System.Runtime.CompilerServices;

namespace SrcGen.Generated;

public static class CallerTypeNameExtensions
{
");

        foreach (var method in methodsToGenerate)
        {
            var callingTypeName = method.ContainingType.ToDisplayString();
            var methodName = method.Name;

            sb.Append($@"
    public static void {methodName}CallerTypeName([CallerMemberName] string callerMemberName = """")
    {{
        {callingTypeName}.{methodName}();
        System.Console.WriteLine($""Called from: {{callerMemberName}} in {{typeof({callingTypeName}).Name}}"");
    }}
");
        }

        sb.Append(@"
}
");

        context.AddSource("CallerTypeNameExtensions.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}
