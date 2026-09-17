using KindRaise.Domain.Campaign;
using Microsoft.EntityFrameworkCore;

namespace KindRaise.Infrastructure.Database
{
    public class KindRaiseDbContext : DbContext
    {
        public KindRaiseDbContext(DbContextOptions<KindRaiseDbContext> options) : base(options)
        {

        }

        public DbSet<Campaign> Campaigns => Set<Campaign>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(KindRaiseDbContext).Assembly);
        }
    }
}
