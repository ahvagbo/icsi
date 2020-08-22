#nullable enable

using System;
using System.Collections.Generic;

#if DEBUG
using System.Diagnostics;
#endif

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace ICsi.Roslyn
{
    internal sealed class SimpleScriptEngine : IScriptEngine
    {
        private ScriptState? _state;

        public ScriptEngineOptions Options { get; }

        internal SimpleScriptEngine(ScriptEngineOptions options)
        {
            _state = null;
            Options = options;
        }

        public ScriptResult Run(string code)
        {
            object? returnValue = null;
            CompilationErrorException? cex = null;
            Exception? eex = null;

            IEnumerable<MetadataReference> GetReferences()
            {
                foreach (string reference in Options.References)
                    yield return MetadataReference.CreateFromFile(reference);
            }

            ScriptOptions options = ScriptOptions.Default
                                                 .WithLanguageVersion(Options.Version)
                                                 .WithAllowUnsafe(Options.AllowUnsafe)
                                                 .WithWarningLevel(Options.WarningLevel)
                                                 .AddReferences(GetReferences())
                                                 .AddImports(Options.Imports);

#if DEBUG
            Debug.WriteLine("Starting script execution...");
#endif

            try
            {
                if (_state == null)
                    _state = CSharpScript.RunAsync(code, options)
                                         .GetAwaiter()
                                         .GetResult();
                else
                    _state = _state.ContinueWithAsync(code, options)
                                   .GetAwaiter()
                                   .GetResult();
                
                returnValue = _state.ReturnValue;
                eex = _state.Exception;

#if DEBUG
                Debug.WriteLine("Script run with success.");
#endif
            }

            catch (CompilationErrorException ex)
            {
#if DEBUG
                Debug.WriteLine("Script compilation failed.");
#endif

                cex = ex;
            }

            catch (Exception) { throw; }

            return new ScriptResult(returnValue,
                                    cex,
                                    eex);
        }
    }
}