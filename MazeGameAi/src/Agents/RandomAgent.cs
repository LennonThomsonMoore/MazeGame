using System;
using System.Collections.Generic;
using System.Text;
using MazeGame.Api.Models;
using MazeGame.Api.Contracts;

namespace MazeGameAi.src.Agents
{
    public class RandomAgent : IAgent
    {
        public Direction decideMove(PollResponse gameState)
        {
            return (Direction)new Random().Next(0, 4);
        }
    }
}
