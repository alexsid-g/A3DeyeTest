using System.Globalization;
using System.Text.RegularExpressions;

namespace FileSorter.Data;

public sealed class FileSortingOptions
{
    public string? InputFilePath { get; set; }

    public string? OutputFilePath { get; set; }

    public string? MemoryLimit { get; set; }

    public string? TempDirectory { get; set; }

    public SortAlgorithm BigFileSortAlgorithm { get; set; } = SortAlgorithm.SortAndSave;

    public static long ParseMemoryLimit(string? memoryLimit)
    {
        if (string.IsNullOrWhiteSpace(memoryLimit))
        {
            return 0;
        }

        Match match = Regex.Match(
            memoryLimit.Trim(),
            @"^(?<value>\d+)\s*(?<unit>MB|GB)$",
            RegexOptions.IgnoreCase);

        if (!match.Success || !long.TryParse(
                match.Groups["value"].Value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out long value))
        {
            return 0;
        }

        long multiplier = match.Groups["unit"].Value.ToUpperInvariant() switch
        {
            "MB" => 1024L * 1024,
            "GB" => 1024L * 1024 * 1024,
            _ => 0
        };

        if (value <= 0 || value > long.MaxValue / multiplier)
        {
            return 0;
        }

        return value * multiplier;
    }
}
