using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

#if DEBUG
using System.Diagnostics;
#endif

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace ICsi.Roslyn
{
    internal sealed class CompiledScriptEngine : IScriptEngine
    {
        private Script<object> _script;

        public ScriptEngineOptions Options { get; }

        public CompiledScriptEngine(ScriptEngineOptions options)
        {
            _script = null;
            Options = options;
        }

        public ScriptResult Run(string code, string[] args)
        {
            object returnValue = null;
            CompilationErrorException cex = null;
            Exception eex = null;

            IEnumerable<MetadataReference> GetReferences()
            {
                foreach (string reference in Options.References)
                    yield return MetadataReference.CreateFromFile(reference, new MetadataReferenceProperties());
            }

            var options = ScriptOptions.Default.WithLanguageVersion(Options.Version)
                                               .WithWarningLevel(Options.WarningLevel)
                                               .WithAllowUnsafe(Options.AllowUnsafe)
                                               .WithImports(Options.Imports)
                                               .WithReferences(GetReferences());

            try
            {
                if (_script == null)
                    _script = CSharpScript.Create(code, options);
                else
                    _script = _script.ContinueWith(code, options);
                
                var scriptCompilation = _script.GetCompilation();

#if DEBUG
                Debug.WriteLine("Compiling script...");
#endif

                using var peStream = new MemoryStream();
                var emitResult = scriptCompilation.Emit(peStream);

                if (!emitResult.Success)
                {
#if DEBUG
                    Debug.WriteLine("Script compilation failed.");
#endif
                    throw new CompilationErrorException("Compilation failed.", emitResult.Diagnostics);
                }

                else
                {
#if DEBUG
                    Debug.WriteLine("Script compiled with success.");
#endif
                    Assembly asm = Assembly.Load(peStream.GetBuffer());
                    Type submission = asm.GetType("Submission#0");
                    MethodInfo factory = submission.GetMethod("<Factory>");

#if DEBUG
                    Debug.WriteLine("Got the entry point method.");
#endif

                    object[] submissionStates = new object[2];
                    submissionStates[1] = args;

#if DEBUG
                    Debug.WriteLine("Invoking entry point method");
#endif

                    Task<object> result = factory.Invoke(null, new[] { submissionStates }) as Task<object>;
                    returnValue = result.GetAwaiter().GetResult();
                }
            }

            catch (CompilationErrorException ex)
            {
                cex = ex;
            }

            catch (Exception ex)
            {
                eex = ex;
            }

            return new ScriptResult(returnValue, cex, eex);
        }

        public ScriptResult Run(string code)
            => Run(code, new string[0]);
    }
}