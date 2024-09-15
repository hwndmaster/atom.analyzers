using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

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
        isEnabledByDefault: true,
        "This method is considered dangerous and should be avoided.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
        context.RegisterOperationAction(AnalyzePropertyReference, OperationKind.PropertyReference);
        context.RegisterOperationAction(AnalyzeNew, OperationKind.ObjectCreation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        ValidateSymbolForUsingDangerousAttribute(context, invocation.TargetMethod);
    }

    private static void AnalyzePropertyReference(OperationAnalysisContext context)
    {
        var propertyReference = (IPropertyReferenceOperation)context.Operation;
        ValidateSymbolForUsingDangerousAttribute(context, propertyReference.Member);
    }

    private static void AnalyzeNew(OperationAnalysisContext context)
    {
        var objectCreation = (IObjectCreationOperation)context.Operation;
        ValidateSymbolForUsingDangerousAttribute(context, objectCreation.Constructor);
    }

    private static void ValidateSymbolForUsingDangerousAttribute(OperationAnalysisContext context, ISymbol? symbol)
    {
        if (symbol is null)
            return;

        AttributeData? dangerousAttribute = symbol.GetAttributes()
            .FirstOrDefault(x => "DangerousAttribute".Equals(x.AttributeClass?.Name));

        if (dangerousAttribute is null)
            return;

        var message = (string?)dangerousAttribute.ConstructorArguments.FirstOrDefault().Value;
        var diagnostic = Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), symbol.Name,
            message);
        context.ReportDiagnostic(diagnostic);
    }
}
