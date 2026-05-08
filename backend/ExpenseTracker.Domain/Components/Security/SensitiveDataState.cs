using ExpenseTracker.Domain.Helpers;

namespace ExpenseTracker.Domain.Components.Security;

public sealed class SensitiveDataState<TSensitive>
    where TSensitive : ISensitiveData
{
    public byte[] EncryptedData { get; private set; } = [];
    public TSensitive? SensitiveData { get; private set; } = default;

    internal void SetEncryptedData(byte[] data)
    {
        Guard.AgainstNull(data, nameof(data));

        if (data.Length is 0)
            throw new ArgumentException(
                $"{nameof(data)} cannot be empty.", nameof(data)
            );

        EncryptedData = data;
    }

    internal void SetSensitiveData(TSensitive data) => SensitiveData = data;
    internal void ClearSensitiveData() => SensitiveData = default;
}
