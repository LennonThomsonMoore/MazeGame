using System;

namespace MazeGame.Api.Contracts
{
    public class RegisterResponse
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Username { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public RegisterResponse(Guid id, string username, DateTimeOffset createdAt)
        {
            this.Id = id;
            this.Username = username;
            this.CreatedAt = createdAt;
        }
    }
}
