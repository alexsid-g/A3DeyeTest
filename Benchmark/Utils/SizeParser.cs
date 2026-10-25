using System.Globalization;
using System.Text.RegularExpressions;

namespace Benchmark.Utils;

public static class SizeParser
{
    public static long Parse(string? size)
    {
        if (string.IsNullOrWhiteSpace(size))
        {
            return 0;
        }

        Match match = Regex.Match(
            size.Trim(),
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
