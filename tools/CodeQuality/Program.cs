using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

// Análise sintática de todos os perfis, sem simular as APIs ou a execução do Unity.
internal static class Program
{
    private static readonly string[][] Profiles =
    {
        Array.Empty<string>(),
        new[] { "UNITY_EDITOR" },
        new[] { "DEVELOPMENT_BUILD" }
    };

    private static bool HasErrors(string source, string[] symbols)
    {
        var options = new CSharpParseOptions(LanguageVersion.CSharp8, preprocessorSymbols: symbols);
        return CSharpSyntaxTree.ParseText(source, options).GetDiagnostics()
            .Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
    }

    private static void SelfCheck()
    {
        if (HasErrors("class Example { string text = \"{;\"; }", Profiles[0]) ||
            !HasErrors("class Example { void Run( }", Profiles[0]) ||
            !HasErrors("#if UNITY_EDITOR\nclass Broken {\n#endif", Profiles[1]) ||
            HasErrors("#if UNITY_EDITOR\nclass Broken {\n#endif", Profiles[0]) ||
            !HasErrors("#if DEVELOPMENT_BUILD\nclass Broken {\n#endif", Profiles[2]))
            throw new InvalidOperationException("A autoverificação do analisador sintático falhou.");
    }

    public static int Main()
    {
        SelfCheck();
        string root = Directory.GetCurrentDirectory();
        string[] folders = { "Assets", "Tests/Core", "tools/CodeQuality" };
        string[] files = folders.SelectMany(folder => Directory.GetFiles(Path.Combine(root, folder), "*.cs", SearchOption.AllDirectories))
            .Where(path => !path.Split(Path.DirectorySeparatorChar).Any(part => part == "obj" || part == "bin"))
            .OrderBy(path => path, StringComparer.Ordinal).ToArray();
        int errors = 0;
        foreach (string path in files)
        {
            string source = File.ReadAllText(path);
            foreach (string[] profile in Profiles)
            {
                var options = new CSharpParseOptions(LanguageVersion.CSharp8, preprocessorSymbols: profile);
                foreach (Diagnostic diagnostic in CSharpSyntaxTree.ParseText(source, options, path).GetDiagnostics()
                    .Where(item => item.Severity == DiagnosticSeverity.Error))
                {
                    Console.Error.WriteLine(diagnostic);
                    errors++;
                }
            }
        }
        Console.WriteLine($"Sintaxe C#: {files.Length} arquivos, {Profiles.Length} perfis, {errors} erros; 5 autoverificações aprovadas.");
        return errors == 0 ? 0 : 1;
    }
}
