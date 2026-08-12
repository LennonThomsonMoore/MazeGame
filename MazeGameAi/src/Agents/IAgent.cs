using MazeGame.Api.Contracts;
using MazeGame.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MazeGameAi.src.Agents
{
    public interface IAgent
    {
        public Direction decideMove(PollResponse gameState);
    }
}
