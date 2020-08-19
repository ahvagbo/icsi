#nullable enable

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ICsi.SyntaxHighlighting
{
    internal static class CSharpSyntaxHighlighter
    {
        internal static object? RenderLine(IReadOnlyList<string> document,
                                           int lineIndex,
                                           object? state)
        {
            string line = document[lineIndex];

            if (line.StartsWith("."))
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(line);
                Console.ResetColor();
            }

            else
            {
                IEnumerable<SyntaxToken> tokens = SyntaxFactory.ParseTokens(line,
                                                                            0,
                                                                            0,
                                                                            CSharpParseOptions.Default
                                                                                              .WithKind(SourceCodeKind.Script));

                foreach (SyntaxToken token in tokens)
                    TokenWriter.Write(token);
            }
            return state;
        }
    }
}