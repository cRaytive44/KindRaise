using Microsoft.EntityFrameworkCore;

namespace KindRaise.Infrastructure.Database.Configurations
{
    public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Type)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(o => o.Content)
                .IsRequired();

            builder.Property(o => o.OccurredAt)
                .IsRequired();

            builder.Property(o => o.PublishedAt);

            builder.Property(o => o.Attempts);

            builder.Property(o => o.LastError)
                .HasMaxLength(1000);

            builder.ToTable("OutboxMessages");

            builder.HasIndex(o => new { o.PublishedAt, o.OccurredAt });
        }
    }
}
