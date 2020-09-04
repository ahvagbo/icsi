using System;
using System.Collections.Generic;
using System.Collections.Immutable;

#if DEBUG
using System.Linq;
using System.Diagnostics;
#endif

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
        private bool _enableSyntaxHighlighting;
        private bool _enableSyntaxVisualizer;

        internal Repl(ReplConfiguration config)
        {
            ScriptEngineOptions options = new ScriptEngineOptions(config.Version,
                                                                  config.WarningLevel,
                                                                  config.AllowUnsafe,
                                                                  config.Imports,
                                                                  config.References);
            _history = new List<string>();
            _historyIndex = 0;
            _scriptEngine = new SimpleScriptEngine(options);
            _enableSyntaxHighlighting = config.EnableSyntaxHighlighting;
            _enableSyntaxVisualizer = false;

            RegisterMetaCommands();
        }

        private  void ReportDiagnostics(ImmutableArray<Diagnostic> diagnostics)
        {
#if DEBUG
            int errors = diagnostics.Where(diag => diag.Severity == DiagnosticSeverity.Error).Count();
            int warnings = diagnostics.Where(diag => diag.Severity == DiagnosticSeverity.Warning).Count();
            int infos = diagnostics.Where(diag => diag.Severity == DiagnosticSeverity.Info).Count();
            Debug.WriteLine($"Report: Diagnostics found in the code - {errors} errors, {warnings} warnings and {infos} informations.");
#endif

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
#if DEBUG
            Debug.WriteLine("Starting the REPL...");
#endif
            while (true)
            {
                string code = EditSubmission();
                
                if (code.StartsWith(".") && !code.Contains(Environment.NewLine))
                    ExecuteCommand(code);
                else
                {
                    ExecuteSubmission(code);
                    
                    if (_enableSyntaxVisualizer)
                    {
                        Console.WriteLine();
                        CSharpSyntaxVisualizer.PrettyPrint(code, _scriptEngine.Options.Version);
                    }
                }
                
                _history.Add(code);
                _historyIndex = 0;
            }
        }
    }
}