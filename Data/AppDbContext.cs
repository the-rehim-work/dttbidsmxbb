using dttbidsmxbb.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace dttbidsmxbb.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser, AppRole, int>(options)
    {
        public DbSet<MilitaryRank> MilitaryRanks { get; set; }
        public DbSet<MilitaryBase> MilitaryBases { get; set; }
        public DbSet<Executor> Executors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
