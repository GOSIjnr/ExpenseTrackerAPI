using System.Text.Json;

namespace ExpenseTracker.Application.Helpers;

internal static class JsonByteArrayConverter
{
    public static byte[] SerializeToUtf8Bytes<T>(T value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        return JsonSerializer.SerializeToUtf8Bytes(value);
    }

    public static T DeserializeFromUtf8Bytes<T>(byte[] utf8Bytes)
    {
        if (utf8Bytes is null || utf8Bytes.Length is 0)
            throw new ArgumentException("Value cannot be null or empty.", nameof(utf8Bytes));

        T? result = JsonSerializer.Deserialize<T>(utf8Bytes)
            ?? throw new InvalidOperationException(
                $"Failed to deserialize data into type {typeof(T).FullName}"
            );

        return result;
    }
}
