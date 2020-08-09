using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;

using static System.Console;

namespace ICsi
{
    internal static class Program
    {
        private static void WriteDiagnostic(Diagnostic diagnostic)
        {
            if (diagnostic.Severity == DiagnosticSeverity.Error)
                ForegroundColor = ConsoleColor.DarkRed;
            else if (diagnostic.Severity == DiagnosticSeverity.Warning)
                ForegroundColor = ConsoleColor.Yellow;
            else
                ForegroundColor = ConsoleColor.DarkGray;
            
            WriteLine(diagnostic);
            ResetColor();
        }

        private static void Main(string[] args)
        {
            ScriptState state = null;
            var parseOptions = CSharpParseOptions.Default
                                                 .WithLanguageVersion(LanguageVersion.Latest)
                                                 .WithKind(SourceCodeKind.Script);
            var scriptOptions = ScriptOptions.Default
                                             .WithLanguageVersion(LanguageVersion.Latest)
                                             .WithWarningLevel(1)
                                             .WithAllowUnsafe(true);
            
            bool IsComplete(string submission)
            {
                var tree = CSharpSyntaxTree.ParseText(submission,
                                                      parseOptions);
                return SyntaxFactory.IsCompleteSubmission(tree);
            }

            while (true)
            {
                Write("> ");
                string code = ReadLine();

                while (!IsComplete(code))
                {
                    Write("* ");
                    string line = ReadLine();
                    code += line;
                }

                try
                {
                    if (state == null)
                        state = CSharpScript.RunAsync(code, scriptOptions)
                                                .GetAwaiter()
                                                .GetResult();
                    else
                        state = state.ContinueWithAsync(code, scriptOptions)
                                         .GetAwaiter()
                                         .GetResult();
                        
                    WriteLine(state.ReturnValue);
                }

                catch (CompilationErrorException cex)
                {
                    foreach (Diagnostic diag in cex.Diagnostics)
                        WriteDiagnostic(diag);
                }

                catch (Exception ex)
                {
                    WriteLine($"{ex.GetType()}: {ex.Message}");
                }
            }
        }
    }
}
