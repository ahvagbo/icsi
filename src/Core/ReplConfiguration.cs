using System.Linq;

using Microsoft.CodeAnalysis.CSharp;

namespace ICsi.Core
{
    internal sealed class ReplConfiguration
    {
        public LanguageVersion Version { get; }
        public bool AllowUnsafe { get; }
        public int WarningLevel { get; }
        public string[] Imports { get; }
        public string[] References { get; }

        internal ReplConfiguration(LanguageVersion version,
                                   bool allowUnsafe,
                                   int warningLevel,
                                   string[] imports,
                                   string[] references)
        {
            Version = version;
            AllowUnsafe = allowUnsafe;
            WarningLevel = warningLevel;
            Imports = imports;
            References = references;
        }

        public static ReplConfiguration Default
            => new ReplConfiguration(LanguageVersion.CSharp9,
                                     false,
                                     1,
                                     new string[] {},
                                     new string[] {});

        public ReplConfiguration WithLanguageVersion(LanguageVersion version)
            => new ReplConfiguration(version,
                                     AllowUnsafe,
                                     WarningLevel,
                                     Imports,
                                     References);
        
        public ReplConfiguration WithWarningLevel(int warningLevel)
            => new ReplConfiguration(Version,
                                     AllowUnsafe,
                                     warningLevel,
                                     Imports,
                                     References);
        
        public ReplConfiguration WithAllowUnsafe(bool allowUnsafe)
            => new ReplConfiguration(Version,
                                     allowUnsafe,
                                     WarningLevel,
                                     Imports,
                                     References);
        
        public void AddImports(string[] imports)
            => Imports.ToList().AddRange(imports);

        public void AddReferences(string[] references)
            => References.ToList().AddRange(references);
    }
}