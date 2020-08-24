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
    [Command("ICsi", UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect,
                     AllowArgumentSeparator = true)]
    [HelpOption("--help | -h | -?")]
    [VersionOptionFromMember("--version | -ver", MemberName = "GetVersion")]
    internal sealed class Application
    {
        private string? _version = "9.0";
        private bool _allowUnsafe = false;
        private int _warningLevel = 1;
        private string[]? _imports = new string[] {};
        private string[]? _references = new string[] {};

        private static string GetVersion()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Version? ver = asm.GetName().Version;
            return $"ICsi version {ver}";
        }

        [Option("--script <SCRIPT_FILE>", "Executes a given script file", CommandOptionType.SingleValue)]
        public string? ScriptFile { get; }

        [Option("--eval | -e <CODE_SNIPPET>", "Executes a given code snippet", CommandOptionType.SingleValue)]
        public string? Eval { get; }

        [Option("--langversion | -langver <VERSION>", "Specifies the C# version", CommandOptionType.SingleValue)]
        public string? LangVersion { get => _version; private set => _version = value; }

        [Option("--unsafe <TRUE_OR_FALSE>", "Specifies whether unsafe blocks are allowed or not", CommandOptionType.SingleValue)]
        public bool AllowUnsafe { get => _allowUnsafe; private set => _allowUnsafe = value; }

        [Option("--warninglevel | -warn <WARN_LEVEL>", "Specifies the warning level", CommandOptionType.SingleValue)]
        public int WarningLevel { get => _warningLevel; private set => _warningLevel = value; }
        
        [Option("--imports | -i <NAMESPACE>", "Imports a namespace", CommandOptionType.MultipleValue)]
        public string[]? Imports { get => _imports; private set => _imports = value; }

        [Option("--reference | -r <METADATA_FILE>", "References a metadata file", CommandOptionType.MultipleValue)]
        public string[]? References { get => _references; private set => _references = value; }

        public string[]? RemainingArguments { get; }

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

        public void OnExecute()
        {
            if (!LanguageVersionFacts.TryParse(LangVersion, out var version))
                Console.Error.WriteLine($"Unrecognized version: {version}");
            
            var engineOptions = ScriptEngineOptions.Default.WithVersion(version)
                                                           .WithAllowUnsafe(AllowUnsafe)
                                                           .WithWarningLevel(WarningLevel);
            engineOptions.AddImports(Imports);
            engineOptions.AddReferences(References);

            if (ScriptFile != null)
            {
                ExecuteFile(ScriptFile, RemainingArguments!, engineOptions);
            }

            else if (Eval != null)
            {
                Execute(Eval, engineOptions);
            }

            else
            {
                ReplConfiguration config = new ReplConfiguration(version,
                                                                 AllowUnsafe,
                                                                 WarningLevel,
                                                                 Imports.ToList(),
                                                                 References.ToList());
                Repl repl = new Repl(config);
                repl.Run();
            }
        }
    }
}