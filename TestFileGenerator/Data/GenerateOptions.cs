using System.Globalization;
using System.Text.RegularExpressions;

namespace TestFileGenerator.Data;

public sealed class GenerateOptions
{
    public string? OutputFilePath { get; set; }

    public string? TargetFileSize { get; set; }

    public static long ParseTargetFileSize(string? targetFileSize)
    {
        if (string.IsNullOrWhiteSpace(targetFileSize))
        {
            return 0;
        }

        Match match = Regex.Match(
            targetFileSize.Trim(),
            @"^(?<value>\d+)(?<unit>MB|GB)$",
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
            _ => 1
        };

        if (value <= 0 || value > long.MaxValue / multiplier)
        {
            return 0;
        }

        return multiplier * value;
    }
}
