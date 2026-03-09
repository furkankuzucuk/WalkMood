using Microsoft.EntityFrameworkCore;
using WalkMood.API.Models;


namespace WalkMood.API.Data
{
    public class WalkMoodDbContext : DbContext
    {
        public WalkMoodDbContext(DbContextOptions<WalkMoodDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<SavedRoute> SavedRoutes { get; set; }
    }
}