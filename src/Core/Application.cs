#nullable enable

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Immutable;

#if DEBUG
using System.Diagnostics;
#endif

using ICsi.Roslyn;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using McMaster.Extensions.CommandLineUtils;

namespace ICsi.Core
{
    internal sealed class Application
    {
        private string GetLongFormVersion()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Version? ver = asm.GetName().Version;
            return $"ICsi version {ver}";
        }

        private string GetShortFormVersion()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Version? ver = asm.GetName().Version;
            return $"{ver}";
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
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                else if (diagnostic.Severity == DiagnosticSeverity.Warning)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else
                    Console.ForegroundColor = ConsoleColor.DarkGray;
            
                Console.WriteLine(diagnostic);
                Console.ResetColor();
            }
        }

        private void ExecuteFile(string fileName, string[] args, ScriptEngineOptions options)
        {
            try
            {
                if (!File.Exists(fileName))
                    Console.Error.WriteLine($"File {fileName} does not exist.");
                else if (Path.GetExtension(fileName) != ".csx")
                    Console.Error.WriteLine($"File {fileName} is not a CSX file.");
                else
                {
                    string fileContent = File.ReadAllText(fileName);
                    var scriptEngine = new SimpleScriptEngine(options);
                    ScriptResult result = scriptEngine.Run(fileContent);

                    if (result.CompilationException != null)
                        ReportDiagnostics(result.CompilationException.Diagnostics);
                    else if (result.ExecutionException != null)
                        Console.WriteLine($"{result.ExecutionException.GetType()} = {result.ExecutionException.Message}");
                    Console.WriteLine(result.ReturnValue);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetType()}: {ex.Message}");
            }
        }

        private void Execute(string code, ScriptEngineOptions options)
        {
            try
            {
                var scriptEngine = new SimpleScriptEngine(options);
                ScriptResult result = scriptEngine.Run(code);

                if (result.CompilationException != null)
                    ReportDiagnostics(result.CompilationException.Diagnostics);
                else if (result.ExecutionException != null)
                    Console.WriteLine($"{result.ExecutionException.GetType()} = {result.ExecutionException.Message}");
                    Console.WriteLine(result.ReturnValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetType()}: {ex.Message}");
            }
        }

        public int Run(string[] args)
        {
            var app = new CommandLineApplication()
            {
                Name = "ICsi",
                Description = "A REPL (read-eval-print-loop) made to run C# code",
                ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated,
                AllowArgumentSeparator = true,
                UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect
            };

            var scriptFile = app.Option<string>("--script <SCRIPT_FILE>",
                                                "Executes a given script file",
                                                CommandOptionType.SingleValue);
            var eval = app.Option<string>("--eval | -e <CODE_SNIPPET>",
                                          "Executes a given code snippet",
                                          CommandOptionType.SingleValue);
            
            var langVersion = app.Option<string>("--langversion | -langver <VERSION>",
                                                 "Specifies the C# version",
                                                 CommandOptionType.SingleValue);
            var allowUnsafe = app.Option<bool>("--unsafe[:<TRUE_OR_FALSE>]",
                                               "Specifies whether unsafe blocks are allowed or not",
                                               CommandOptionType.SingleOrNoValue);
            var warningLevel = app.Option<int>("--warninglevel | -warn <WARN_LEVEL>",
                                               "Specifies the warning level",
                                               CommandOptionType.SingleValue);
            var imports = app.Option<string>("--imports | -i",
                                             "Imports a namespace or adds a static import",
                                             CommandOptionType.MultipleValue);
            var reference = app.Option<string>("--reference | -r",
                                               "References a metadata file",
                                               CommandOptionType.MultipleValue);

            app.HelpOption("--help | -h | -?");
            app.VersionOption("--version | -ver",
                              GetShortFormVersion(),
                              GetLongFormVersion());
            
            app.OnExecute(() => {
                if (!LanguageVersionFacts.TryParse(langVersion.Value(), out var version))
                    Console.Error.WriteLine($"Unrecognied version: {langVersion.Value()}");
                
                var engineOptions = ScriptEngineOptions.Default.WithVersion(version)
                                                           .WithAllowUnsafe(allowUnsafe.ParsedValue)
                                                           .WithWarningLevel(warningLevel.ParsedValue);
                engineOptions.AddImports(imports.Values.ToArray());
                engineOptions.AddReferences(reference.Values.ToArray());

                if (scriptFile.HasValue())
                    ExecuteFile(scriptFile.ParsedValue,
                                app.RemainingArguments.ToArray(),
                                engineOptions);
                
                else if (eval.HasValue())
                    Execute(eval.ParsedValue,
                            engineOptions);

                else
                {
                    var replConfiguration = new ReplConfiguration(version,
                                                                  allowUnsafe.ParsedValue,
                                                                  warningLevel.ParsedValue,
                                                                  imports.Values.ToList(),
                                                                  reference.Values.ToList());
                    var repl = new Repl(replConfiguration);
                    repl.Run();
                }
            });

            return app.Execute(args);
        }
    }
}