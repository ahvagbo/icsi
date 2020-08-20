using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ICsi.SyntaxHighlighting
{
    internal static class TokenWriter
    {
        public static void Write(SyntaxToken token)
        {
            if (token.HasLeadingTrivia)
            {
                foreach (SyntaxTrivia trivia in token.LeadingTrivia)
                {
                    if (trivia.HasStructure)
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    else if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                          || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
                          || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                          || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                        Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(trivia.ToFullString());
                }

                Console.ResetColor();
            }

            if (token.IsKeyword())
                Console.ForegroundColor = ConsoleColor.Blue;
            else if (token.IsKind(SyntaxKind.StringLiteralToken)
                  || token.IsKind(SyntaxKind.InterpolatedStringToken)
                  || token.IsKind(SyntaxKind.CharacterLiteralToken)
                  || token.IsVerbatimStringLiteral())
                Console.ForegroundColor = ConsoleColor.Red;
            else if (token.IsKind(SyntaxKind.IdentifierToken))
                Console.ForegroundColor = ConsoleColor.DarkGray;
            else if (token.IsKind(SyntaxKind.NumericLiteralToken))
                Console.ForegroundColor = ConsoleColor.Cyan;
            else if (SyntaxFacts.IsAnyOverloadableOperator(token.Kind())
                  || SyntaxFacts.IsAssignmentExpressionOperatorToken(token.Kind()))
                Console.ForegroundColor = ConsoleColor.Magenta;
            else
                Console.ForegroundColor = ConsoleColor.Gray;

            Console.Write(token.Text);
            Console.ResetColor();

            if (token.HasTrailingTrivia)
            {
                foreach (SyntaxTrivia trivia in token.TrailingTrivia)
                {
                    if (trivia.HasStructure)
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    else if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                          || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
                          || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                          || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                        Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(trivia.ToFullString());
                }
                
                Console.ResetColor();
            }
        }
    }
}