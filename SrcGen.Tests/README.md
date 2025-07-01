# SrcGen.Tests

This project contains unit tests for the `HelloWorldGenerator` source generator.

## Test Structure

- **HelloWorldGeneratorTests.cs**: Contains unit tests using the Microsoft.CodeAnalysis.CSharp.SourceGenerators.Testing framework
- **HelloWorldGeneratorIntegrationTests.cs**: Contains integration tests that verify the generator works with actual compilation

## Running Tests

To run the tests, use one of these commands:

```bash
# From the SrcGen.Tests directory
dotnet test

# From the solution root
dotnet test SrcGen.Tests

# Run with detailed output
dotnet test --logger:console;verbosity=detailed
```

## Test Coverage

The tests cover:
- ✅ Single method with attribute generates correct extension method
- ✅ Multiple methods with attribute generate multiple extension methods  
- ✅ Methods without attribute generate no output
- ✅ Non-method elements with attribute generate no output
- ✅ Instance methods generate extension methods
- ✅ Integration tests verify end-to-end compilation
