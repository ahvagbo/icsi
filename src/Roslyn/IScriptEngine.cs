namespace ICsi.Roslyn
{
    internal interface IScriptEngine
    {
        ScriptEngineOptions Options { get; }
        ScriptResult Run(string code);
    }
}