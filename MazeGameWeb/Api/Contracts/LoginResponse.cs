using System;

namespace MazeGame.Api.Contracts
{
    public class LoginResponse
    {
        public string Username { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }

        public LoginResponse(string username, DateTimeOffset expiresAt)
        {
            this.Username = username;
            this.ExpiresAt = expiresAt;
        }
    }
}
