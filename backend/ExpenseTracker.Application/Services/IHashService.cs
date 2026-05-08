namespace ExpenseTracker.Application.Services;

public interface IHashService
{
    string Hash(string input);
    bool Verify(string input, string hashedValue);
}
