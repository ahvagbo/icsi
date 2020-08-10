using Microsoft.CodeAnalysis.CSharp;

namespace ICsi.Roslyn
{
    internal sealed class ScriptEngineOptions
    {
        public LanguageVersion Version { get; }
        public int WarningLevel { get; }
        public bool AllowUnsafe { get; }

        internal ScriptEngineOptions(LanguageVersion version,
                                     int warningLevel,
                                     bool allowUnsafe)
        {
            Version = version;
            WarningLevel = warningLevel;
            AllowUnsafe = allowUnsafe;
        }

        public static ScriptEngineOptions Default
            => new ScriptEngineOptions(version: LanguageVersion.CSharp9,
                                       warningLevel: 1,
                                       allowUnsafe: false);

        public ScriptEngineOptions WithVersion(LanguageVersion version)
            => new ScriptEngineOptions(version, WarningLevel, AllowUnsafe);
        
        public ScriptEngineOptions WithWarningLevel(int warningLevel)
            => new ScriptEngineOptions(Version, warningLevel, AllowUnsafe);
        
        public ScriptEngineOptions WithAllowUnsafe(bool allowUnsafe)
            => new ScriptEngineOptions(Version, WarningLevel, allowUnsafe);
    }
}