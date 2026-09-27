using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

internal static class Program
{
    private static int Main()
    {
        var root = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName;
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(Path.Combine(root, "src")))
        {
            Console.Error.WriteLine("Repository root could not be resolved.");
            return 2;
        }

        var sourceRoot = Path.Combine(root, "src", "CFIP.Indicator");
        var csFiles = Directory.GetFiles(sourceRoot, "*.cs", SearchOption.AllDirectories);
        var declaration = Path.Combine(sourceRoot, "Indicator", "CFIPIndicator.Declarations.cs");

        if (!File.Exists(declaration))
        {
            Console.Error.WriteLine("Missing CFIPIndicator.Declarations.cs");
            return 3;
        }

        var declarationText = File.ReadAllText(declaration);
        var parameterCount = Regex.Matches(declarationText, @"[Parameters*(").Count;
        if (parameterCount != 512)
        {
            Console.Error.WriteLine($"Expected 512 parameters, found {parameterCount}.");
            return 4;
        }

        var partialNames = new[]
        {
            "Runtime", "Execution", "Metrics", "BrokerEvents",
            "Lifecycle", "Identity"
        };

        foreach (var name in partialNames)
        {
            var path = Path.Combine(sourceRoot, "Indicator", $"CFIPIndicator.{name}.cs");
            if (!File.Exists(path))
            {
                Console.Error.WriteLine($"Missing host partial: {path}");
                return 5;
            }
        }

        var indicatorFiles = Directory.GetFiles(
                Path.Combine(sourceRoot, "Indicator"),
                "CFIPIndicator.*.cs",
                SearchOption.TopDirectoryOnly)
            .Select(File.ReadAllText)
            .ToArray();

        var manualEntrySurface = indicatorFiles.Any(t =>
            Regex.IsMatch(t, @"(Button|AddButton|TradeButton)", RegexOptions.IgnoreCase) &&
            Regex.IsMatch(t, @"(Buy|Sell|Stop|Limit)", RegexOptions.IgnoreCase));

        if (manualEntrySurface)
        {
            Console.Error.WriteLine("Manual trade-entry control surface detected.");
            return 6;
        }

        if (csFiles.Length < 107)
        {
            Console.Error.WriteLine($"Expected at least 107 migrated C# files, found {csFiles.Length}.");
            return 7;
        }

        Console.WriteLine($"Static migration verification passed. C# files: {csFiles.Length}; parameters: {parameterCount}.");
        return 0;
    }
}
