using FluentValidation;
using ExpenseTracker.Domain.Entities.Users;

namespace ExpenseTracker.Application.Features.Users.GetUsers;

internal sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    private const int MaxPageSize = 100;

    public GetUsersQueryValidator()
    {
        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit must be greater than 0.")
            .LessThanOrEqualTo(MaxPageSize)
            .WithMessage($"Limit cannot exceed {MaxPageSize}.");

        RuleFor(x => x.UserName)
            .Cascade(CascadeMode.Stop)
            .MinimumLength(1)
                .WithMessage($"Username must be at least {1} characters long.")
            .MaximumLength(UserLimits.UserNameMaxLength)
                .WithMessage($"Username must not exceed {UserLimits.UserNameMaxLength} characters.")
            .Matches(UserLimits.UserNameRegex())
                .WithMessage("Username may only contain letters, numbers, underscores, and hyphens.");
    }
}
