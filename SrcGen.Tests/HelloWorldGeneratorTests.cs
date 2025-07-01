using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SrcGen;
using System.Collections.Immutable;
using System.Reflection;
using Xunit;

namespace SrcGen.Tests;

public class HelloWorldGeneratorTests
{
    [Fact]
    public void Generator_WithMethodWithAttribute_GeneratesExtensionMethod()
    {
        // Arrange
        var source = """
            using System;
            using SrcGen;

            namespace TestNamespace
            {
                public class TestClass
                {
                    [MySpecial]
                    public static void TestMethod()
                    {
                        Console.WriteLine("Hello from TestMethod");
                    }
                }
            }

            namespace SrcGen
            {
                [System.AttributeUsage(System.AttributeTargets.Method)]
                public class MySpecialAttribute : System.Attribute
                {
                }
            }
            """;

        // Act
        var generatedSource = RunSourceGenerator(source);

        // Assert
        Assert.Contains("CallerTypeNameExtensions", generatedSource);
        Assert.Contains("TestMethodCallerTypeName", generatedSource);
        Assert.Contains("TestNamespace.TestClass.TestMethod()", generatedSource);
        Assert.Contains("CallerMemberName", generatedSource);
    }

    [Fact]
    public void Generator_WithMultipleMethodsWithAttribute_GeneratesMultipleExtensionMethods()
    {
        // Arrange
        var source = """
            using System;
            using SrcGen;

            namespace TestNamespace
            {
                public class TestClass
                {
                    [MySpecial]
                    public static void FirstMethod()
                    {
                        Console.WriteLine("Hello from FirstMethod");
                    }

                    [MySpecial]
                    public static void SecondMethod()
                    {
                        Console.WriteLine("Hello from SecondMethod");
                    }
                }
            }

            namespace SrcGen
            {
                [System.AttributeUsage(System.AttributeTargets.Method)]
                public class MySpecialAttribute : System.Attribute
                {
                }
            }
            """;

        // Act
        var generatedSource = RunSourceGenerator(source);

        // Assert
        Assert.Contains("FirstMethodCallerTypeName", generatedSource);
        Assert.Contains("SecondMethodCallerTypeName", generatedSource);
        Assert.Contains("TestNamespace.TestClass.FirstMethod()", generatedSource);
        Assert.Contains("TestNamespace.TestClass.SecondMethod()", generatedSource);
    }

    [Fact]
    public void Generator_WithNoAttributedMethods_GeneratesNoOutput()
    {
        // Arrange
        var source = """
            using System;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public static void TestMethod()
                    {
                        Console.WriteLine("Hello from TestMethod");
                    }
                }
            }
            """;

        // Act
        var generatedSource = RunSourceGenerator(source);

        // Assert
        Assert.Null(generatedSource);
    }

    [Fact]
    public void Generator_WithNonMethodAttribute_GeneratesNoOutput()
    {
        // Arrange
        var source = """
            using System;
            using SrcGen;

            namespace TestNamespace
            {
                [MySpecial]
                public class TestClass
                {
                    public static void TestMethod()
                    {
                        Console.WriteLine("Hello from TestMethod");
                    }
                }
            }

            namespace SrcGen
            {
                [System.AttributeUsage(System.AttributeTargets.All)]
                public class MySpecialAttribute : System.Attribute
                {
                }
            }
            """;

        // Act
        var generatedSource = RunSourceGenerator(source);

        // Assert
        Assert.Null(generatedSource);
    }

    [Fact]
    public void Generator_WithInstanceMethod_GeneratesExtensionMethod()
    {
        // Arrange
        var source = """
            using System;
            using SrcGen;

            namespace TestNamespace
            {
                public class TestClass
                {
                    [MySpecial]
                    public void InstanceMethod()
                    {
                        Console.WriteLine("Hello from InstanceMethod");
                    }
                }
            }

            namespace SrcGen
            {
                [System.AttributeUsage(System.AttributeTargets.Method)]
                public class MySpecialAttribute : System.Attribute
                {
                }
            }
            """;

        // Act
        var generatedSource = RunSourceGenerator(source);

        // Assert
        Assert.Contains("InstanceMethodCallerTypeName", generatedSource);
        Assert.Contains("TestNamespace.TestClass.InstanceMethod()", generatedSource);
    }

    [Fact]
    public void Generator_CompilationSucceeds()
    {
        // Arrange
        var source = """
            using System;
            using SrcGen;

            namespace TestNamespace
            {
                public class TestClass
                {
                    [MySpecial]
                    public static void TestMethod()
                    {
                        Console.WriteLine("Hello from TestMethod");
                    }
                }
            }

            namespace SrcGen
            {
                [System.AttributeUsage(System.AttributeTargets.Method)]
                public class MySpecialAttribute : System.Attribute
                {
                }
            }
            """;

        // Act
        var (compilation, diagnostics) = RunSourceGeneratorWithCompilation(source);

        // Assert
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        
        var generatedTrees = compilation.SyntaxTrees.Skip(1); // Skip the original source
        Assert.Single(generatedTrees);
    }

    private string? RunSourceGenerator(string source)
    {
        var (compilation, _) = RunSourceGeneratorWithCompilation(source);
        var generatedTrees = compilation.SyntaxTrees.Skip(1); // Skip the original source
        
        if (!generatedTrees.Any())
            return null;
            
        return generatedTrees.First().GetText().ToString();
    }

    private (Compilation compilation, ImmutableArray<Diagnostic> diagnostics) RunSourceGeneratorWithCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location)
        };

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new HelloWorldGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        return (outputCompilation, diagnostics);
    }
}
