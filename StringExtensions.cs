namespace BoneBaker;

public static class StringExtensions
{
    public static bool IsNullOrWhitespace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }
}