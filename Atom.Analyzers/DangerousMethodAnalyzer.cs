using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Genius.Atom.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DangerousMethodAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        "ATOM0001",
        "Dangerous Method Usage",
        "Method '{0}' is marked as dangerous: {1}",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;
        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);

        if (methodSymbol == null)
            return;

        var dangerousAttribute = methodSymbol.GetAttributes().FirstOrDefault(attr =>
            attr.AttributeClass.Name == "DangerousAttribute");

        if (dangerousAttribute is not null)
        {
            var message = (string?)dangerousAttribute.ConstructorArguments.FirstOrDefault().Value;
            var diagnostic = Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation(), methodSymbol.Name, message);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
