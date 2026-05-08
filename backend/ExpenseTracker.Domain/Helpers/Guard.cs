using System.Text.RegularExpressions;

namespace ExpenseTracker.Domain.Helpers;

internal static class Guard
{
    public static T AgainstNull<T>(T? value, string parameterName)
        where T : class
    {
        if (value is null)
            throw new ArgumentNullException(parameterName);

        return value;
    }

    public static string AgainstNullOrWhiteSpace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                $"{parameterName} cannot be null, empty, or whitespace.",
                parameterName
            );

        return value;
    }

    public static Guid AgainstEmptyGuid(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
            throw new ArgumentException(
                $"{parameterName} must be a non-empty GUID.",
                parameterName
            );

        return value;
    }

    public static string AgainstInvalidLength(string value, int minLength, int maxLength, string parameterName)
    {
        if (value.Length < minLength || value.Length > maxLength)
            throw new ArgumentException(
                $"{parameterName} must be between {minLength} and {maxLength} characters.",
                parameterName
            );

        return value;
    }

    public static string AgainstInvalidMaxLength(string value, int maxLength, string parameterName)
    {
        if (value.Length > maxLength)
            throw new ArgumentException(
                $"{parameterName} must not exceed {maxLength} characters.",
                parameterName
            );

        return value;
    }

    public static string AgainstInvalidExactLength(string value, int exactLength, string parameterName)
    {
        if (value.Length != exactLength)
            throw new ArgumentException(
                $"{parameterName} must be exactly {exactLength} characters.",
                parameterName
            );

        return value;
    }

    public static string AgainstInvalidFormat(string value, Regex regex, string parameterName)
    {
        if (!regex.IsMatch(value))
            throw new ArgumentException(
                $"{parameterName} is not in the expected format.",
                parameterName
            );

        return value;
    }

    public static T AgainstOutOfRange<T>(T value, T min, T max, string parameterName)
        where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                $"{parameterName} must be between {min} and {max}."
            );

        return value;
    }

    public static T AgainstInvalidEnum<T>(T value, string parameterName)
        where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new ArgumentException(
                $"{parameterName} has an invalid value.",
                parameterName
            );

        return value;
    }

    public static DateTime AgainstNonFutureDate(DateTime value, string parameterName)
    {
        if (value <= DateTime.UtcNow)
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                $"{parameterName} must be a future date."
            );

        return value;
    }
}
