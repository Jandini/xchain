using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xchain.Generators;

[Generator]
public sealed class CollectionChainGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor DuplicateStep = new(
        id: "XCHAIN001",
        title: "Step type used in multiple chains",
        messageFormat: "Step type '{0}' appears in multiple WorkflowChain configurations ({1}). Each step must belong to exactly one chain.",
        category: "Xchain",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var allChains = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax cls && cls.BaseList is not null,
                transform: static (ctx, ct) => GetChainInfo(ctx, ct))
            .Where(static x => x is not null)
            .Collect();

        context.RegisterSourceOutput(allChains, static (ctx, infos) =>
        {
            var stepToChains = new Dictionary<string, List<string>>();
            foreach (var info in infos)
                foreach (var fqn in GetPrimaryStepFqns(info!))
                {
                    if (!stepToChains.TryGetValue(fqn, out var names))
                        stepToChains[fqn] = names = new List<string>();
                    names.Add(info!.DisplayName);
                }

            var duplicates = new HashSet<string>();
            foreach (var pair in stepToChains)
                if (pair.Value.Count > 1)
                {
                    duplicates.Add(pair.Key);
                    ctx.ReportDiagnostic(Diagnostic.Create(
                        DuplicateStep, Location.None, pair.Key, string.Join(", ", pair.Value)));
                }

            foreach (var info in infos)
            {
                var (hintName, source) = Generate(info!, duplicates);
                ctx.AddSource(hintName, source);
            }
        });
    }

    private static ChainInfo? GetChainInfo(GeneratorSyntaxContext ctx, CancellationToken ct)
    {
        var cls = (ClassDeclarationSyntax)ctx.Node;
        if (ctx.SemanticModel.GetDeclaredSymbol(cls, ct) is not INamedTypeSymbol symbol)
            return null;
        if (!IsCollectionChain(symbol))
            return null;

        var configureMethod = cls.Members
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.Text == "Configure");
        if (configureMethod is null)
            return null;

        var calls = ExtractCalls(configureMethod, ctx.SemanticModel, ct);
        if (calls.Count == 0)
            return null;

        var displayName = symbol.ToDisplayString();
        var hintName = displayName
            .Replace('.', '_')
            .Replace('<', '_').Replace('>', '_')
            .Replace(',', '_').Replace('+', '_');
        return new ChainInfo(hintName, displayName, calls);
    }

    private static bool IsCollectionChain(INamedTypeSymbol symbol)
    {
        var base_ = symbol.BaseType;
        while (base_ is not null)
        {
            if (base_.Name == "WorkflowChain" &&
                base_.ContainingNamespace?.ToDisplayString() == "Xchain")
                return true;
            base_ = base_.BaseType;
        }
        return false;
    }

    private static List<CallInfo> ExtractCalls(
        MethodDeclarationSyntax method,
        SemanticModel semanticModel,
        CancellationToken ct)
    {
        ExpressionSyntax? expr = method.ExpressionBody?.Expression;
        if (expr is null)
        {
            if (method.Body?.Statements.FirstOrDefault() is ExpressionStatementSyntax stmt)
                expr = stmt.Expression;
        }
        if (expr is null) return new List<CallInfo>();

        var collected = new List<CallInfo>();
        SyntaxNode current = expr;

        while (current is InvocationExpressionSyntax invocation &&
               invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            ct.ThrowIfCancellationRequested();
            var methodName = memberAccess.Name.Identifier.Text;
            var types = new List<ITypeSymbol>();

            if (memberAccess.Name is GenericNameSyntax generic)
            {
                foreach (var typeArg in generic.TypeArgumentList.Arguments)
                {
                    var typeInfo = semanticModel.GetTypeInfo(typeArg, ct);
                    if (typeInfo.Type is not null)
                        types.Add(typeInfo.Type);
                }
            }

            collected.Add(new CallInfo(methodName, types));
            current = memberAccess.Expression;
        }

        collected.Reverse();
        return collected;
    }

    private static IEnumerable<string> GetPrimaryStepFqns(ChainInfo info)
    {
        foreach (var call in info.Calls)
        {
            if (call.MethodName == "After") continue;
            if (call.Types.Count > 0)
                yield return call.Types[0].ToDisplayString();
        }
    }

    private static (string HintName, string Source) Generate(ChainInfo info, HashSet<string> duplicates)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#pragma warning disable CS0436");
        sb.AppendLine();

        var pendingAwaits = new List<ITypeSymbol>();
        ITypeSymbol? prev = null;
        var steps = new List<StepDef>();

        foreach (var call in info.Calls)
        {
            if (call.MethodName == "After")
            {
                pendingAwaits.AddRange(call.Types);
                continue;
            }
            if (call.Types.Count == 0) continue;

            var stepType = call.Types[0];
            var isStart = call.MethodName == "Start";
            steps.Add(new StepDef(stepType, isStart, prev, new List<ITypeSymbol>(pendingAwaits)));
            pendingAwaits.Clear();
            prev = stepType;
        }

        var byNs = steps
            .GroupBy(s => s.Type.ContainingNamespace?.ToDisplayString() ?? string.Empty)
            .ToList();

        foreach (var nsGroup in byNs)
        {
            var ns = nsGroup.Key;
            bool hasNs = !string.IsNullOrEmpty(ns);

            if (hasNs)
            {
                sb.AppendLine($"namespace {ns}");
                sb.AppendLine("{");
            }

            foreach (var step in nsGroup)
                AppendStep(sb, step, hasNs ? "    " : "", duplicates);

            if (hasNs)
                sb.AppendLine("}");

            sb.AppendLine();
        }

        return ($"CollectionChain_{info.ClassName}.g.cs", sb.ToString());
    }

    private static void AppendStep(StringBuilder sb, StepDef step, string indent, HashSet<string> duplicates)
    {
        var typeName = step.Type.Name;
        var fqn = step.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var collectionKey = step.Type.ToDisplayString();

        if (duplicates.Contains(collectionKey))
            return;

        sb.AppendLine($"{indent}[global::Xunit.CollectionAttribute(\"{collectionKey}\")]");
        sb.AppendLine($"{indent}public partial class {typeName} {{ }}");
        sb.AppendLine();
        sb.AppendLine($"{indent}[global::Xunit.CollectionDefinitionAttribute(\"{collectionKey}\")]");

        bool useInline = step.PendingAwaits.Count > 0;

        if (!useInline && step.IsStart)
        {
            sb.AppendLine($"{indent}public class {typeName}Definition");
            sb.AppendLine($"{indent}    : global::Xchain.CollectionChainStartDefinition<{fqn}> {{ }}");
        }
        else if (!useInline)
        {
            var prevFqn = step.Prev!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            sb.AppendLine($"{indent}public class {typeName}Definition");
            sb.AppendLine($"{indent}    : global::Xchain.CollectionChainNextDefinition<{prevFqn}, {fqn}> {{ }}");
        }
        else
        {
            sb.AppendLine($"{indent}public class {typeName}Definition");
            bool first = true;

            foreach (var awaitType in step.PendingAwaits)
            {
                var awaitFqn = awaitType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var sep = first ? "    :" : "    ,";
                sb.AppendLine($"{indent}{sep} global::Xunit.ICollectionFixture<global::Xchain.CollectionChainAwait<{awaitFqn}>>");
                first = false;
            }

            if (!step.IsStart && step.Prev is not null)
            {
                var prevFqn = step.Prev.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var sep = first ? "    :" : "    ,";
                sb.AppendLine($"{indent}{sep} global::Xunit.ICollectionFixture<global::Xchain.CollectionChainAwait<{prevFqn}>>");
                first = false;
            }

            var sigSep = first ? "    :" : "    ,";
            sb.AppendLine($"{indent}{sigSep} global::Xunit.ICollectionFixture<global::Xchain.CollectionChainSignalFixture<{fqn}>>");
            sb.AppendLine($"{indent}    , global::Xunit.ICollectionFixture<global::Xchain.CollectionChainContextFixture> {{ }}");
        }

        sb.AppendLine();
    }

    private sealed record ChainInfo(string ClassName, string DisplayName, List<CallInfo> Calls);
    private sealed record CallInfo(string MethodName, List<ITypeSymbol> Types);
    private sealed record StepDef(ITypeSymbol Type, bool IsStart, ITypeSymbol? Prev, List<ITypeSymbol> PendingAwaits);
}
