using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdPhotoManager.Core.Entities;

namespace AdPhotoManager.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.AdObjectId)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.AdObjectId)
            .IsUnique();

        builder.Property(u => u.DisplayName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.EmployeeId)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(u => u.EmployeeId);

        builder.Property(u => u.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Organization)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.Organization);

        builder.Property(u => u.Department)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.Department);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.Email);

        builder.Property(u => u.HasPhoto)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.LastSyncedAt)
            .IsRequired();

        builder.Property(u => u.PhotoUpdatedAt)
            .IsRequired(false);

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(u => u.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(u => u.IsDeleted)
            .HasFilter("[IsDeleted] = 0");
    }
}
