using Microsoft.EntityFrameworkCore;

namespace KindRaise.Infrastructure.Database
{
    public class KindRaiseDbContext : DbContext
    {
        public KindRaiseDbContext(DbContextOptions<KindRaiseDbContext> options) : base(options)
        {

        }
    }
}
