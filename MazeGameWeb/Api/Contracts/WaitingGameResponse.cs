using MazeGame.Api.Models;

namespace MazeGame.Api.Contracts
{
    public class WaitingGameResponse
    {
        public Guid GameId { get; set; }
        public GameStatus GameStatus { get; set; }

        public WaitingGameResponse(Guid gameId, GameStatus gameStatus)
        {
            GameId = gameId;
            GameStatus = gameStatus;
        }
    }
}
