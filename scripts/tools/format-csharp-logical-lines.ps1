[CmdletBinding(DefaultParameterSetName='Files')]
param([Parameter(Mandatory, ParameterSetName='Files')][string[]]$LiteralPath, [Parameter(Mandatory, ParameterSetName='Directory')][string]$Directory, [switch]$Check)
$ErrorActionPreference = 'Stop'
if ($PSCmdlet.ParameterSetName -eq 'Directory') { $LiteralPath = @(Get-ChildItem -LiteralPath $Directory -Recurse -File -Filter *.cs | ForEach-Object { $_.FullName }) }
foreach ($path in $LiteralPath) { if (-not (Test-Path -LiteralPath $path -PathType Leaf) -or [System.IO.Path]::GetExtension($path) -ne '.cs') { throw "Fichier C# introuvable : $path" } }
$compilerAssemblies = @([AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.GetName().Name -in @('Microsoft.CodeAnalysis', 'Microsoft.CodeAnalysis.CSharp') })
if ($compilerAssemblies.Count -ne 2) { throw 'Les assemblies du parseur C# de PowerShell ne sont pas chargées.' }
$references = @($compilerAssemblies | ForEach-Object { $_.Location }) + @([System.Collections.Immutable.ImmutableArray].Assembly.Location, [System.Linq.Enumerable].Assembly.Location)
if (-not ('LogicalCSharpLines' -as [type])) {
Add-Type -ReferencedAssemblies $references -CompilerOptions '/nowarn:1701,1702' -TypeDefinition @"
using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class LogicalCSharpLines
{
    public static string Format(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview));
        if (tree.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error)) throw new InvalidOperationException("Syntaxe C# invalide avant traitement.");
        var root = tree.GetRoot();
        var text = tree.GetText();
        var changes = root.DescendantTrivia().Where(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia) && !text.Lines.GetLineFromPosition(trivia.SpanStart).ToString().Contains("//") && Compact(trivia)).ToArray();
        var positions = changes.Select(trivia => trivia.Span.End).ToArray();
        var indentation = root.DescendantTrivia().Where(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia) && positions.Contains(trivia.SpanStart)).ToArray();
        var result = root.ReplaceTrivia(changes.Concat(indentation), (original, rewritten) => original.IsKind(SyntaxKind.EndOfLineTrivia) ? SyntaxFactory.Space : SyntaxFactory.Whitespace(string.Empty));
        if (!root.DescendantTokens().Select(token => (token.RawKind, token.Text)).SequenceEqual(result.DescendantTokens().Select(token => (token.RawKind, token.Text)))) throw new InvalidOperationException("Modification des jetons C# refusée.");
        var formatted = result.ToFullString();
        if (CSharpSyntaxTree.ParseText(formatted, new CSharpParseOptions(LanguageVersion.Preview)).GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error)) throw new InvalidOperationException("Syntaxe C# invalide après traitement.");
        return formatted;
    }

    private static bool Compact(SyntaxTrivia trivia)
    {
        var next = trivia.Token.GetNextToken();
        if (next.IsKind(SyntaxKind.OpenBraceToken) || next.IsKind(SyntaxKind.CloseBraceToken)) return false;
        if (next.IsKind(SyntaxKind.ColonToken) && (next.Parent is ConstructorInitializerSyntax || next.Parent is BaseListSyntax)) return true;
        foreach (var node in trivia.Token.Parent.AncestorsAndSelf())
        {
            if (node is BlockSyntax || node is InitializerExpressionSyntax) return false;
            if (node is StatementSyntax || node is ParameterListSyntax || node is ArgumentListSyntax || node is BracketedArgumentListSyntax || node is ArrowExpressionClauseSyntax || node is FieldDeclarationSyntax || node is EqualsValueClauseSyntax || node is BaseListSyntax || node is TypeParameterListSyntax || node is BinaryExpressionSyntax || node is ConditionalExpressionSyntax) { if (trivia.SpanStart >= node.SpanStart && trivia.SpanStart < node.Span.End) return true; }
        }
        return false;
    }
}
"@
}
$changed = 0
foreach ($path in $LiteralPath) {
    $resolved = (Resolve-Path -LiteralPath $path).Path
    $original = [System.IO.File]::ReadAllText($resolved)
    $formatted = [LogicalCSharpLines]::Format($original)
    if ($original -cne $formatted) { $changed++; if (-not $Check) { [System.IO.File]::WriteAllText($resolved, $formatted, [System.Text.UTF8Encoding]::new($false)) } }
}
if ($Check -and $changed -gt 0) { throw "$changed fichier(s) contiennent des instructions coupées." }
Write-Output "$changed fichier(s) regroupé(s)."
