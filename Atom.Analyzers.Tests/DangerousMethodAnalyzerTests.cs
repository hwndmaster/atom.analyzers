using Microsoft.CodeAnalysis.Testing;

namespace Genius.Atom.Analyzers.Tests;

public sealed class DangerousMethodAnalyzerTests
{
    [Fact]
    public async Task NoDiagnosticForSafeMethod()
    {
        var test = @"
            using System;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void SafeMethod()
                    {
                    }
                }
            }";

        await Verify.VerifyAnalyzerAsync<DangerousMethodAnalyzer>(test);
    }

    [Fact]
    public async Task DiagnosticForDangerousMethod()
    {
        var test = @"
            using System;
            using Genius.Atom.Infrastructure.Attributes;

            namespace TestNamespace
            {
                public class TestClass
                {
                    [Dangerous(""This method is dangerous"")]
                    public void DangerousMethod()
                    {
                    }

                    public void TriggerMethod()
                    {
                        DangerousMethod();
                    }
                }
            }";

        var expected = new DiagnosticResult(DangerousMethodAnalyzer.Rule)
            .WithSpan(16, 25, 16, 42)
            .WithArguments("DangerousMethod", "This method is dangerous");

        await Verify.VerifyAnalyzerAsync<DangerousMethodAnalyzer>(test, expected);
    }
}
