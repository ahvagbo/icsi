using Microsoft.CodeAnalysis.Scripting;

namespace ICsi.Roslyn
{
    internal interface IScriptEngine
    {
        ScriptState State { get; }
        ScriptEngineOptions Options { get; }
        ScriptResult Run(string code);
    }
}