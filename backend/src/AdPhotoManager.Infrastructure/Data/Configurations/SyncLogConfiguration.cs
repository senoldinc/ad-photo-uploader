using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdPhotoManager.Core.Entities;

namespace AdPhotoManager.Infrastructure.Data.Configurations;

public class SyncLogConfiguration : IEntityTypeConfiguration<SyncLog>
{
    public void Configure(EntityTypeBuilder<SyncLog> builder)
    {
        builder.ToTable("SyncLogs");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.StartedAt)
            .IsRequired();

        builder.HasIndex(s => s.StartedAt)
            .IsDescending();

        builder.Property(s => s.CompletedAt)
            .IsRequired(false);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.HasIndex(s => s.Status);

        builder.Property(s => s.UsersProcessed)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.UsersAdded)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.UsersUpdated)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.UsersDeleted)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.ErrorMessage)
            .HasMaxLength(4000)
            .IsRequired(false);

        builder.Property(s => s.ErrorDetails)
            .IsRequired(false);

        builder.Property(s => s.TriggerType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.TriggeredBy)
            .HasMaxLength(255)
            .IsRequired(false);
    }
}
