using MazeGame.Api.Models;
using System.Runtime.CompilerServices;

namespace MazeGame.Api.Contracts
{
    public class GamesWaitingResponse
    {
        public List<Game> Games { get; set; }

        public GamesWaitingResponse(List<Game> Games)
        {
            this.Games = Games;
        }
    }
}
