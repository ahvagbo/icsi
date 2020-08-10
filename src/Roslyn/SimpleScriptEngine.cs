#nullable enable

using System;

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

            ScriptOptions options = ScriptOptions.Default
                                                 .WithLanguageVersion(Options.Version)
                                                 .WithAllowUnsafe(Options.AllowUnsafe)
                                                 .WithWarningLevel(Options.WarningLevel);

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
            }

            catch (CompilationErrorException ex)
            {
                cex = ex;
            }

            catch (Exception) { throw; }

            return new ScriptResult(returnValue,
                                    cex,
                                    eex);
        }
    }
}