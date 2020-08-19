using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using ICsi.Roslyn;

using Microsoft.CodeAnalysis;

using static System.Console;

namespace ICsi.Core
{
    internal sealed partial class Repl
    {
        private readonly IList<string> _history;
        private int _historyIndex;
        private SimpleScriptEngine _scriptEngine;

        internal Repl()
        {
            _history = new List<string>();
            _historyIndex = 0;
            _scriptEngine = new SimpleScriptEngine(ScriptEngineOptions.Default);
            RegisterMetaCommands();
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

        private void ExecuteSubmission(string code)
        {
            try
            {
                ScriptResult result = _scriptEngine.Run(code);

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

        public void Run()
        {
            while (true)
            {
                string code = EditSubmission();
                
                if (code.StartsWith(".") && !code.Contains(Environment.NewLine))
                    ExecuteCommand(code);
                else
                    ExecuteSubmission(code);
                
                _history.Add(code);
                _historyIndex = 0;
            }
        }
    }
}