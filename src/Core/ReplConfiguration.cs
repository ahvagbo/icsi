using System.Collections.Generic;

using Microsoft.CodeAnalysis.CSharp;

namespace ICsi.Core
{
    internal sealed class ReplConfiguration
    {
        public LanguageVersion Version { get; }
        public bool AllowUnsafe { get; }
        public int WarningLevel { get; }
        public List<string> Imports { get; }
        public List<string> References { get; }
        public bool EnableSyntaxHighlighting { get; }

        internal ReplConfiguration(LanguageVersion version,
                                   bool allowUnsafe,
                                   int warningLevel,
                                   List<string> imports,
                                   List<string> references,
                                   bool enableSyntaxHighlighting)
        {
            Version = version;
            AllowUnsafe = allowUnsafe;
            WarningLevel = warningLevel;
            Imports = imports;
            References = references;
            EnableSyntaxHighlighting = enableSyntaxHighlighting;
        }

        public static ReplConfiguration Default
            => new ReplConfiguration(LanguageVersion.CSharp9,
                                     false,
                                     1,
                                     new List<string> {},
                                     new List<string> {},
                                     true);

        public ReplConfiguration WithLanguageVersion(LanguageVersion version)
            => new ReplConfiguration(version,
                                     AllowUnsafe,
                                     WarningLevel,
                                     Imports,
                                     References,
                                     EnableSyntaxHighlighting);
        
        public ReplConfiguration WithWarningLevel(int warningLevel)
            => new ReplConfiguration(Version,
                                     AllowUnsafe,
                                     warningLevel,
                                     Imports,
                                     References,
                                     EnableSyntaxHighlighting);
        
        public ReplConfiguration WithAllowUnsafe(bool allowUnsafe)
            => new ReplConfiguration(Version,
                                     allowUnsafe,
                                     WarningLevel,
                                     Imports,
                                     References,
                                     EnableSyntaxHighlighting);

        public ReplConfiguration WithEnableSyntaxHighlighting(bool enableSyntaxHighlighting)
            => new ReplConfiguration(Version,
                                     AllowUnsafe,
                                     WarningLevel,
                                     Imports,
                                     References,
                                     enableSyntaxHighlighting);
        
        public void AddImports(string[] imports)
            => Imports.AddRange(imports);

        public void AddReferences(string[] references)
            => References.AddRange(references);
    }
}