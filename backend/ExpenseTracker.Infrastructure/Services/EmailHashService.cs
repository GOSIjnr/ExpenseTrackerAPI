using System.Security.Cryptography;
using System.Text;
using ExpenseTracker.Application.Services;
using Microsoft.Extensions.Options;
using ExpenseTracker.Infrastructure.Configurations.Security.Hashing;

namespace ExpenseTracker.Infrastructure.Services;

internal sealed class EmailHashService(IOptions<HashingOptions> options) : IHashService
{
    private readonly byte[] _key = Convert.FromBase64String(options.Value.EmailHmacKey);

    public string Hash(string input)
    {
        using HMACSHA256 hmac = new(_key);
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));

        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public bool Verify(string input, string hashedValue)
    {
        byte[] expected = Convert.FromHexString(hashedValue);

        using HMACSHA256 hmac = new(_key);
        byte[] actual = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
