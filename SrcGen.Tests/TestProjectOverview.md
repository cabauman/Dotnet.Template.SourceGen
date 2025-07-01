# SrcGen.Tests - Test Project Overview

This test project validates the functionality of the `HelloWorldGenerator` source generator using comprehensive unit tests.

## Project Structure

```
SrcGen.Tests/
├── SrcGen.Tests.csproj          # Test project configuration
├── HelloWorldGeneratorTests.cs  # Main test file with unit tests
├── HelloWorldGeneratorIntegrationTests.cs # Integration tests (currently unused)
├── GlobalUsings.cs              # Global using statements
└── README.md                    # This documentation
```

## Test Cases Covered

### ✅ Core Functionality Tests

1. **Single Method with Attribute** - Verifies that a method decorated with `[MySpecial]` generates the correct extension method
2. **Multiple Methods with Attribute** - Tests multiple decorated methods generate multiple extension methods
3. **No Attributed Methods** - Ensures no output is generated when no methods have the attribute
4. **Non-Method Attribute** - Verifies that attributes on non-method elements don't trigger generation
5. **Instance Method** - Tests that instance methods generate extension methods correctly
6. **Compilation Success** - Integration test ensuring the generated code compiles without errors

### 🔧 Test Implementation Details

The tests use a custom helper method `RunSourceGenerator()` that:
- Creates a `CSharpCompilation` with the test source code
- Executes the `HelloWorldGenerator` 
- Returns the generated source code for validation
- Validates compilation succeeds without errors

### 📦 Dependencies

- **Microsoft.CodeAnalysis.CSharp** (4.14.0) - For running the source generator
- **xunit** (2.9.2) - Test framework
- **Microsoft.NET.Test.Sdk** (17.12.0) - Test SDK
- **coverlet.collector** (6.0.2) - Code coverage

## Running Tests

```bash
# From solution root
dotnet test

# From test project directory
cd SrcGen.Tests
dotnet test

# With detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test
dotnet test --filter "DisplayName~Generator_WithMethodWithAttribute"
```

## Expected Test Output

All 6 tests should pass:
- ✅ Generator_WithMethodWithAttribute_GeneratesExtensionMethod
- ✅ Generator_WithMultipleMethodsWithAttribute_GeneratesMultipleExtensionMethods  
- ✅ Generator_WithNoAttributedMethods_GeneratesNoOutput
- ✅ Generator_WithNonMethodAttribute_GeneratesNoOutput
- ✅ Generator_WithInstanceMethod_GeneratesExtensionMethod
- ✅ Generator_CompilationSucceeds

## Adding New Tests

To add new test cases:

1. Create a new `[Fact]` method in `HelloWorldGeneratorTests.cs`
2. Use the `RunSourceGenerator(string source)` helper method
3. Assert the expected behavior using xunit assertions
4. Consider both positive and negative test cases

## Future Enhancements

Potential areas for additional testing:
- Generic method support
- Method overloads
- Different access modifiers
- Namespace variations
- Error handling scenarios
- Performance testing
