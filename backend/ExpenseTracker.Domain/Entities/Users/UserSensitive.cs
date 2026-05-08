using System.Net.Mail;
using System.Text.Json.Serialization;
using ExpenseTracker.Domain.Components.Security;
using ExpenseTracker.Domain.Helpers;

namespace ExpenseTracker.Domain.Entities.Users;

public sealed class UserSensitive : ISensitiveData
{
    [JsonConstructor]
    private UserSensitive(string firstName, string? middleName, string lastName, string email)
    {
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        Email = email;
    }

    public string FirstName { get; private set; } = string.Empty;
    public string? MiddleName { get; private set; }
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    public static UserSensitive Create(string firstName, string? middleName, string lastName, string email)
    {
        string validatedFirstName = ValidateRequiredName(firstName, nameof(firstName));
        string? validatedMiddleName = ValidateOptionalName(middleName, nameof(middleName));
        string validatedLastName = ValidateRequiredName(lastName, nameof(lastName));
        string validatedEmail = ValidateEmail(email, nameof(email));

        return new UserSensitive(validatedFirstName, validatedMiddleName, validatedLastName, validatedEmail);
    }

    public void UpdateName(string firstName, string? middleName, string lastName)
    {
        FirstName = ValidateRequiredName(firstName, nameof(firstName));
        MiddleName = ValidateOptionalName(middleName, nameof(middleName));
        LastName = ValidateRequiredName(lastName, nameof(lastName));
    }

    private static string ValidateRequiredName(string name, string paramName)
    {
        string value = Guard.AgainstNullOrWhiteSpace(name, paramName);
        Guard.AgainstInvalidLength(value, UserLimits.NameMinLength, UserLimits.NameMaxLength, paramName);
        Guard.AgainstInvalidFormat(value, UserLimits.NameRegex(), paramName);

        return value;
    }

    private static string? ValidateOptionalName(string? name, string paramName)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        Guard.AgainstInvalidLength(name, UserLimits.NameMinLength, UserLimits.NameMaxLength, paramName);
        Guard.AgainstInvalidFormat(name, UserLimits.NameRegex(), paramName);

        return name;
    }

    private static string ValidateEmail(string email, string paramName)
    {
        string value = Guard.AgainstNullOrWhiteSpace(email, paramName).ToLowerInvariant();
        Guard.AgainstInvalidMaxLength(value, UserLimits.EmailMaxLength, paramName);
        Guard.AgainstInvalidFormat(value, UserLimits.EmailRegex(), paramName);

        if (!IsValidEmail(value))
            throw new ArgumentException("Email format is invalid", paramName);

        return value;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            MailAddress emailAddress = new(email);
            return emailAddress.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
