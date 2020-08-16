using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using ICsi.Roslyn;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using static System.Console;

namespace ICsi.Core
{
    internal sealed partial class Repl
    {
        private readonly IList<string> _history;
        private int _historyIndex;

        internal Repl()
        {
            _history = new List<string>();
            _historyIndex = 0;
        }

        private  void ReportDiagnostics(ImmutableArray<Diagnostic> diagnostics)
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

        public void Run()
        {
            var engineOptions = ScriptEngineOptions.Default;
            var engine = new SimpleScriptEngine(engineOptions);
            var parseOptions = CSharpParseOptions.Default
                                                 .WithLanguageVersion(engineOptions.Version)
                                                 .WithKind(SourceCodeKind.Script);
            while (true)
            {
                string code = EditSubmission();
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
                
                _history.Add(code);
                _historyIndex = 0;
            }
        }
    }
}