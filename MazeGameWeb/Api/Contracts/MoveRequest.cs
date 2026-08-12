using System;
using System.Collections.Generic;
using System.Text;
using MazeGame.Api.Models;

namespace MazeGame.Api.Contracts
{
    public class MoveRequest
    {
        public Guid GameId { get; set; } = Guid.Empty;
        public Guid PlayerToken { get; set; } = Guid.Empty;
        public Direction Direction { get; set; } = Direction.North;

        public MoveRequest(Guid GameId, Guid PlayerToken, Direction Direction)
        {
            this.GameId = GameId;
            this.PlayerToken = PlayerToken;
            this.Direction = Direction;
        }

    }
}
