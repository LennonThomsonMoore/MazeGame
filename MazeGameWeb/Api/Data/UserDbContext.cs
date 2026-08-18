using MazeGame.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MazeGame.Api.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(g => g.UpdatedAt)
                .IsConcurrencyToken();

        }
    }
}
