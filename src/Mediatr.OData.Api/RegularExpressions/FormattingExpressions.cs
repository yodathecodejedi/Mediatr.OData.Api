using System.Text.RegularExpressions;

namespace Mediatr.OData.Api.RegularExpressions;

public static partial class FormattingExpressions
{
    [GeneratedRegex("^(?!\").*(?<!\")$")]
    public static partial Regex NoQuotes();
}
