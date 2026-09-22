using KindRaise.Domain.Campaign;
using KindRaise.Domain.Donation;
using Microsoft.EntityFrameworkCore;

namespace KindRaise.Infrastructure.Database
{
    public class KindRaiseDbContext : DbContext
    {
        public KindRaiseDbContext(DbContextOptions<KindRaiseDbContext> options) : base(options)
        {

        }

        public DbSet<Campaign> Campaigns => Set<Campaign>();
        public DbSet<Donation> Donations => Set<Donation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(KindRaiseDbContext).Assembly);
        }
    }
}
