using KindRaise.Domain.Campaign;
using KindRaise.Domain.Donation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KindRaise.Infrastructure.Database.Configurations
{
    public sealed class DonationConfiguration : IEntityTypeConfiguration<Donation>
    {
        public void Configure(EntityTypeBuilder<Donation> builder) 
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.CampaignId)
                .IsRequired();

            builder.Property(d => d.DonorName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(d => d.Amount)
                .HasPrecision(9, 2);

            builder.Property(d => d.CreatedAt);

            builder.Property(d => d.ProcessingState)
                .HasConversion<string>()
                .IsRequired();

            builder.HasOne<Campaign>()
                .WithMany()
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("Donations", table =>
            {
                table.HasCheckConstraint(
                    "CK_Donation_Amount_Positive",
                    "\"Amount\" > 0");
            });
        }
    }
}
