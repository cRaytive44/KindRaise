using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KindRaise.Infrastructure.Database.Configurations
{
    public sealed class ProcessedMessageConfiguration : IEntityTypeConfiguration<ProcessedMessage>
    {
        public void Configure(EntityTypeBuilder<ProcessedMessage> builder)
        {
            builder.HasKey(p => p.MessageId);

            builder.Property(p => p.MessageId)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(p => p.ProcessedAt)
                .IsRequired();

            builder.ToTable("ProcessedMessages");
        }
    }
}
