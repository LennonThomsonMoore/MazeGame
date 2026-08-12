using MazeGame.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MazeGame.Api.Contracts
{
    public class CreateGameResponse
    {
        public Guid gameId { get; set; } = Guid.Empty;
        public Guid playerToken { get; set; } = Guid.Empty;
        public PlayerType role { get; set; } = PlayerType.Hider;
        public GameStatus status { get; set; } = GameStatus.WaitingForPlayer;

        public CreateGameResponse(Guid gameId, Guid playerToken, PlayerType role, GameStatus status)
        {
            this.gameId = gameId;
            this.playerToken = playerToken;
            this.role = role;
            this.status = status;
        }
    }
}
