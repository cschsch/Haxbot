namespace Web.Data;

public static class StringExtensions
{
    public static string ToCamelCase(this string s)
    {
        return string.Concat(s.First().ToString().ToLower(), s[1..]);
    }
}