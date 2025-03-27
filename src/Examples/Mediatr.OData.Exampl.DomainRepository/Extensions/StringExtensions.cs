using System.Text;

namespace Mediatr.OData.Exampl.DomainRepository.Extensions;

internal static class StringExtensions
{
    public static string Sanitize(this string value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        List<string> lines = [.. value.Split(Environment.NewLine, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)];
        if (lines.Count == 0) throw new InvalidDataException($"there is no data to Sanitize.");
        return string.Join(Environment.NewLine, lines);
    }

    public static bool EndsWithSemiColon(this StringBuilder builder)
    {
        return builder.ToString().EndsWith(';');
    }
    public static bool EndsWithNewLine(this StringBuilder builder)
    {
        return builder.ToString().EndsWith(Environment.NewLine);
    }
}
