using KindRaise.Domain.Campaign;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KindRaise.Infrastructure.Database.Configurations
{
    public sealed class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
    {
        public void Configure(EntityTypeBuilder<Campaign> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Title)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.Description)
                .HasMaxLength(5000)
                .IsRequired();

            builder.Property(c => c.MonetaryGoal)
                .HasPrecision(9, 2);

            builder.Property(c => c.StartDate);

            builder.Property(c => c.EndDate);

            builder.Property(c => c.DonatedAmount)
                .HasPrecision(9, 2);

            builder.Ignore(c => c.CampaignState);

            builder.ToTable("Campaigns", table =>
            {
                table.HasCheckConstraint(
                    "CK_Campaign_MonetaryGoal_Positive",
                    "\"MonetaryGoal\" > 0");

                table.HasCheckConstraint(
                    "CK_Campaign_DonatedAmount_Positive",
                    "\"DonatedAmount\" >= 0");
            });
        }
    }
}
