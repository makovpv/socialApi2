using Microsoft.EntityFrameworkCore;

namespace SPA.Models
{
    public class SocialContext: DbContext
    {
        public SocialContext(DbContextOptions<SocialContext> options)
            : base(options)
        {

        }

        public DbSet<VK_KeyGroups> Vk_keygroups { get; set; }
        public DbSet<VK_Like> Vk_like{ get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VK_Like>()
                .HasKey(c => new { c.ObjectId, c.UserId });
        }
    }
}
