namespace ExpenseTracker.Infrastructure.Configurations.Security.DataEncryption;

public sealed record DataEncryptionOptions
{
    public byte CurrentKeyVersion { get; init; }
    public Dictionary<byte, string> Keys { get; init; } = [];
}
