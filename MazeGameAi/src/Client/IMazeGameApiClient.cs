using MazeGame.Api.Contracts;
using MazeGame.Api.Models;
using MazeGameAi.src.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MazeGameAi.src.Client
{
    public interface IMazeGameApiClient
    {
        public Task<PollResponse> PollAsync(Guid playerToken, Guid gameId);

        public Task<MoveResult> MoveAsync(Guid playerToken, Guid gameId, Direction direction);

        public Task<CreateGameResponse> CreateGameAsync();

        public Task<JoinGameResponse> JoinGameAsync(Guid gameId);

    }
}
