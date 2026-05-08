using BCryption = BCrypt.Net.BCrypt;
using ExpenseTracker.Application.Services;
using Microsoft.Extensions.Options;
using ExpenseTracker.Infrastructure.Configurations.Security.Hashing;

namespace ExpenseTracker.Infrastructure.Services;

internal sealed class PasswordHashService(IOptions<HashingOptions> options) : IHashService
{
    private readonly int _workFactor = options.Value.PasswordWorkFactor;

    public string Hash(string input)
        => BCryption.HashPassword(input, _workFactor);

    public bool Verify(string input, string hashedValue)
        => BCryption.Verify(input, hashedValue);
}
