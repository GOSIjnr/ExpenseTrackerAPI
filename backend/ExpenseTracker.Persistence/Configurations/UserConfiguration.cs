using ExpenseTracker.Domain.Components.Auditing;
using ExpenseTracker.Domain.Components.Security;
using ExpenseTracker.Domain.Entities.Users;
using ExpenseTracker.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.OwnsOne(u => u.AuditState, audit =>
        {
            audit.Property(a => a.CreatedAt)
                .HasColumnName(nameof(AuditState.CreatedAt).ToSnakeCase());

            audit.Property(a => a.UpdatedAt)
                .HasColumnName(nameof(AuditState.UpdatedAt).ToSnakeCase());
        });

        builder.OwnsOne(u => u.SensitiveDataState, sensitive =>
        {
            sensitive.Property(s => s.EncryptedData)
                .HasColumnName(nameof(SensitiveDataState<>.EncryptedData).ToSnakeCase());

            sensitive.Ignore(s => s.SensitiveData);
        });

        builder.HasIndex(u => u.UserName)
            .IsUnique()
            .HasFilter($"\"{nameof(User.IsDeleted).ToSnakeCase()}\" = false");

        builder.HasIndex(u => u.EmailHash)
            .IsUnique()
            .HasFilter($"\"{nameof(User.IsDeleted).ToSnakeCase()}\" = false");

        builder.Property(u => u.UserName)
            .HasMaxLength(UserLimits.UserNameMaxLength);

        builder.Property(u => u.EmailHash)
            .HasMaxLength(UserLimits.EmailHashLength);

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(UserLimits.PasswordHashLength);

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(30);
    }
}
