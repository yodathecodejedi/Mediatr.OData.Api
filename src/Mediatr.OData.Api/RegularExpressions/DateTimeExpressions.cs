using System.Text.RegularExpressions;

namespace Mediatr.OData.Api.RegularExpressions;

public static partial class DateTimeExpressions
{
    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}$")]  // Matches datetime without quotes  yyyy-MM-ddTHH:mm:ss exact
    public static partial Regex ISO8601_T_Value();

    [GeneratedRegex(@"\""\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\""")] // Matches datetime with quotes "yyyy-MM-ddTHH:mm:ss" exact
    public static partial Regex ISO8601_T_Raw();

    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}$")] // Matches datetime without quotes yyyy-MM-dd HH:mm:ss exact
    public static partial Regex ISO8601_Space_Value();

    [GeneratedRegex(@"\""\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\""")] // Matches datetime with quotes "yyyy-MM-dd HH:mm:ss" exact
    public static partial Regex ISO8601_Space_Raw();

    [GeneratedRegex(@"\w{3} \w{3} \d{1,2} \d{4} \d{2}:\d{2}:\d{2} \w{3}[+-]\d{4}$")] // Matches datetime without quotes "ddd MMM dd yyyy HH:mm:ss GMT+0100" exact
    public static partial Regex Javascript_Value();

    [GeneratedRegex(@"\""\w{3} \w{3} \d{1,2} \d{4} \d{2}:\d{2}:\d{2} \w{3}[+-]\d{4}\""")] // Matches datetime with quotes "ddd MMM dd yyyy HH:mm:ss GMT+0100" exact
    public static partial Regex Javascript_Raw(); // Matches datetime with quotes "ddd MMM dd yyyy HH:mm:ss GMT+0100" exact

    [GeneratedRegex(@"^\d{1,2}/\d{1,2}/\d{4} \d{2}:\d{2}:\d{2} \w{2}$")] // Matches datetime without quotes "MM/dd/yyyy HH:mm:ss PM" exact
    public static partial Regex US_Value();

    [GeneratedRegex(@"\""\d{1,2}/\d{1,2}/\d{4} \d{2}:\d{2}:\d{2} \w{2}\""")] // Matches datetime without quotes "MM/dd/yyyy HH:mm:ss PM" exact
    public static partial Regex US_Raw();

    [GeneratedRegex(@"^\d{1,2}-\d{1,2}-\d{4} \d{2}:\d{2}:\d{2}$")] // Matches datetime without quotes "dd-MM-yyyy HH:mm:ss" exact
    public static partial Regex EU_Value();

    [GeneratedRegex(@"\""\d{1,2}-\d{1,2}-\d{4} \d{2}:\d{2}:\d{2}\""")] // Matches datetime without quotes "dd-MM-yyyy HH:mm:ss" exact
    public static partial Regex EU_Raw();
}
