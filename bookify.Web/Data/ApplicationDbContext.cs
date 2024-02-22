using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace bookify.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
		public DbSet<Author> Authors { get; set; }
		public DbSet<Category> Categories { get; set; }
    }
}