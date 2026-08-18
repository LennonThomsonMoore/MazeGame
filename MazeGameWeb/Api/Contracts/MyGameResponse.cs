using MazeGame.Api.Models;

namespace MazeGame.Api.Contracts
{
    public class MyGameResponse
    {
        public Guid GameId { get; set; }
        public PlayerType Role { get; set; }
        public GameStatus GameStatus { get; set; }
        public PlayerType? Winner { get; set; }
        public Guid? PlayerToken { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public MyGameResponse(Guid gameId, PlayerType role, GameStatus gameStatus, PlayerType? winner, Guid? playerToken, DateTimeOffset createdAt, DateTimeOffset updatedAt)
        {
            GameId = gameId;
            Role = role;
            GameStatus = gameStatus;
            Winner = winner;
            PlayerToken = playerToken;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }
    }
}
