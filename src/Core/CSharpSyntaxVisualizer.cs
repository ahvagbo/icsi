using System;
using System.Linq;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ICsi.Core
{
    internal static class CSharpSyntaxVisualizer
    {
        private static string _cross = " ├─";
        private static string _corner = " └─";
        private static string _vertical = " │ ";
        private static string _space = "   ";

        private static void WriteLeadingTrivia(ImmutableArray<SyntaxTrivia> leadingTrivia, string indent)
        {
            foreach (SyntaxTrivia trivia in leadingTrivia)
            {
                Console.Write(indent);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Leading Trivia: {trivia.Kind()}");
                Console.ResetColor();
            }
        }

        private static void WriteTrailingTrivia(ImmutableArray<SyntaxTrivia> trailingTrivia, string indent)
        {
            foreach (SyntaxTrivia trivia in trailingTrivia)
            {
                Console.Write(indent);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Trailing Trivia: {trivia.Kind()}");
                Console.ResetColor();
            }
        }

        private static void WriteToken(SyntaxToken token, string indent)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(token.Kind());
            Console.ResetColor();

            if (token.HasLeadingTrivia)
                WriteLeadingTrivia(token.LeadingTrivia.ToImmutableArray(), indent);
            if (token.HasTrailingTrivia)
                WriteTrailingTrivia(token.TrailingTrivia.ToImmutableArray(), indent);
        }

        private static void WriteNode(SyntaxNode node, string indent)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(node.Kind());
            Console.ResetColor();

            foreach (SyntaxNodeOrToken nodeOrToken in node.ChildNodesAndTokens())
            {
                bool isLast = node.ChildNodesAndTokens().LastOrDefault() == nodeOrToken;
                WriteNodeOrToken(nodeOrToken, indent, isLast);
            }
        }
        
        private static void WriteNodeOrToken(SyntaxNodeOrToken nodeOrToken, string indent, bool isLast)
        {
            Console.Write(indent);

            if (isLast)
            {
                Console.Write(_corner);
                indent += _space;
            }

            else
            {
                Console.Write(_cross);
                indent += _vertical;
            }

            if (nodeOrToken.IsNode)
                WriteNode(nodeOrToken.AsNode(), indent);
            else if (nodeOrToken.IsToken)
                WriteToken(nodeOrToken.AsToken(), indent);
        }

        public static void PrettyPrint(string code, LanguageVersion version, string indent = "")
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code, CSharpParseOptions.Default.WithLanguageVersion(version)
                                                                                         .WithKind(SourceCodeKind.Script));
            CompilationUnitSyntax root = (CompilationUnitSyntax)tree.GetRoot();
            WriteNode(root, "");
        }
    }
}