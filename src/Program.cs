using System;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using ICsi.Roslyn;

using static System.Console;

namespace ICsi
{
    internal static class Program
    {
        private static void ReportDiagnostics(ImmutableArray<Diagnostic> diagnostics)
        {
            foreach (Diagnostic diagnostic in diagnostics)
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
        }

        private static void Main(string[] args)
        {
            var engineOptions = ScriptEngineOptions.Default;
            var engine = new SimpleScriptEngine(engineOptions);
            var parseOptions = CSharpParseOptions.Default
                                                 .WithLanguageVersion(engineOptions.Version)
                                                 .WithKind(SourceCodeKind.Script);
            
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
                    ScriptResult result = engine.Run(code);

                    if (result.CompilationException != null)
                        ReportDiagnostics(result.CompilationException.Diagnostics);
                    else if (result.ExecutionException != null)
                        WriteLine($"{result.ExecutionException.GetType()} = {result.ExecutionException.Message}");
                    else
                        WriteLine(result.ReturnValue);
                }

                catch (Exception ex)
                {
                    WriteLine($"{ex.GetType()}: {ex.Message}");
                }
            }
        }
    }
}
