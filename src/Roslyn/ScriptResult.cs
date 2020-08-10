#nullable enable

using System;

using Microsoft.CodeAnalysis.Scripting;

namespace ICsi.Roslyn
{
    internal sealed class ScriptResult
    {
        public object? ReturnValue { get; }
        public CompilationErrorException? CompilationException { get; }
        public Exception? ExecutionException { get; }

        internal ScriptResult(object? returnValue,
                              CompilationErrorException? compilationEx,
                              Exception? executionEx)
        {
            ReturnValue = returnValue;
            CompilationException = compilationEx;
            ExecutionException = executionEx;
        }
    }
}