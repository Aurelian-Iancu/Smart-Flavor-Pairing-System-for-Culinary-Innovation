using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class AppDbContext: IdentityDbContext<AppUser>
    {
        public AppDbContext()
        {

        }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<UserLike> UserLikes { get; set; }
        public DbSet<Improvement> Improvements { get; set; }
    }
}
