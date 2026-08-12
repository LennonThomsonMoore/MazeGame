using MazeGame.Api.Models;
using MazeGame.Api.Contracts;
using MazeGameAi.src.PathFinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace MazeGameAi.src.Agents
{
    public class SeekerAgent : IAgent
    {   
        public Direction decideMove(PollResponse gameState)
        {
            Dijkstra dijkstra = new Dijkstra();
            return dijkstra.NextMove(true, gameState);
        }

    }
}
