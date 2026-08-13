using System;

namespace MazeGame.Api.Contracts
{
    public class PollRequest
    {
        public Guid? GameId { get; set; }
        public Guid? PlayerToken { get; set; }

        public PollRequest(Guid? GameId, Guid? PlayerToken)
        {
            this.GameId = GameId;
            this.PlayerToken = PlayerToken;
        }
    }
}
