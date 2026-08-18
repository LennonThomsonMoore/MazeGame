using MazeGame.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MazeGame.Api.Data
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }
        public DbSet<Game> Games { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>()
                .OwnsOne(g => g.HiderPosition);

            modelBuilder.Entity<Game>()
                .OwnsOne(g => g.SeekerPosition);

            modelBuilder.Entity<Game>()
                .Property(g => g.UpdatedAt)
                .IsConcurrencyToken();

            modelBuilder.Entity<Game>()
                .Property(g => g.Maze)
                .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Cell[][]>(v, (JsonSerializerOptions)null))
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<Cell[][]>(
                    (c1, c2) => JsonSerializer.Serialize(c1, (JsonSerializerOptions)null) == JsonSerializer.Serialize(c2, (JsonSerializerOptions)null),
                    c => c == null ? 0 : JsonSerializer.Serialize(c, (JsonSerializerOptions)null).GetHashCode(),
                    c => JsonSerializer.Deserialize<Cell[][]>(JsonSerializer.Serialize(c, (JsonSerializerOptions)null), (JsonSerializerOptions)null)));

        }
    }
}
