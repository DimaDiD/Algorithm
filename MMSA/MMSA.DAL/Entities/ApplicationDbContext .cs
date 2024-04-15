using Microsoft.EntityFrameworkCore;

namespace MMSA.DAL.Entities
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Page> Page { get; set; }
    }
}
