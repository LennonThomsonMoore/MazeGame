using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MazeGame.Api.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public User() { }

        public static User CreateUser(string username, string password)
        {
            Guid id = Guid.NewGuid();
            var passwordHasher = new PasswordHasher<String>();

            string passwordHash = passwordHasher.HashPassword(username, password);

            return new User(id, username, passwordHash);
        }

        private User(Guid id, string username, string passwordHash)
        {
            this.Id = id;
            this.Username = username;
            this.PasswordHash = passwordHash;
            this.CreatedAt = DateTimeOffset.UtcNow;
            this.UpdatedAt = DateTimeOffset.UtcNow;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not User other)
            {
                return false;
            }
            return this.Id == other.Id;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
