using System;

public static class StringUtls
{
    public static bool IsEmpty(this string value)
    {
        return value == null || value == string.Empty;
    }

    public static bool IsNotEmpty(this string value)
    {
        return !value.IsEmpty();
    }

    public static bool IsEmptyOrWhiteSpace(this string value)
    {
        return value == null || value.Trim() == string.Empty;
    }
}