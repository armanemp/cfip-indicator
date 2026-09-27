using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

internal static class Program
{
    private static int Main()
    {
        var root = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName;
        if (string.IsNullOrWhiteSpace(root)) return Fail("Repository root could not be resolved.");

        var sourceRoot = Path.Combine(root, "src", "CFIP.Indicator");
        var files = Directory.GetFiles(sourceRoot, "*.cs", SearchOption.AllDirectories);
        var text = string.Join("\n", files.Select(File.ReadAllText));

        if (Regex.IsMatch(text, @"CFIPClean\d+|Clean\d+|\bv\d+\b|CFIP\d+\||CLEAN\d+", RegexOptions.IgnoreCase))
            return Fail("Versioned naming/token detected in source.");

        var expected = new[]
        {
            "Core/Domain.cs",
            "Market/Market.cs",
            "Analysis/Structure.cs",
            "Decision/Decision.cs",
            "Planning/Planning.cs",
            "Risk/Risk.cs",
            "Execution/Execution.cs",
            "Infrastructure/CTrader/Broker.cs",
            "Lifecycle/Lifecycle.cs",
            "LiveManagement/LiveManagement.cs",
            "Outcomes/Outcomes.cs",
            "Presentation/Presentation.cs",
            "Configuration/Configuration.cs",
            "Indicator/CFIPIndicator.Declarations.cs",
            "Indicator/EngineState.cs"
        };

        foreach (var relative in expected)
            if (!File.Exists(Path.Combine(sourceRoot, relative)))
                return Fail("Missing module: " + relative);

        var parameterCount = Regex.Matches(
            File.ReadAllText(Path.Combine(sourceRoot, "Indicator", "CFIPIndicator.Declarations.cs")),
            @"\[Parameter\s*\(").Count;

        if (parameterCount != 512)
            return Fail("Expected 512 public parameters, found " + parameterCount + ".");

        Console.WriteLine("Static architecture verification passed. Source files: " + files.Length + "; parameters: " + parameterCount + ".");
        return 0;
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine(message);
        return 1;
    }
}
