using Microsoft.EntityFrameworkCore;

namespace Lib
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=postgres;Port=5442;Database=usersdb;Username=postgres;Password=postgres");
        }
    }
}
