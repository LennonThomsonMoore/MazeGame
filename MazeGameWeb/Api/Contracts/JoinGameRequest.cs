using System;
using System.Collections.Generic;
using System.Text;

namespace MazeGame.Api.Contracts
{
    public class JoinGameRequest
    {
        public Guid GameId { get; set; } = Guid.Empty;
        public JoinGameRequest(Guid GameId)
        {
            this.GameId = GameId;
        }
    }
}
