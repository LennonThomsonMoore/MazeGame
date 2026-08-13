using MazeGame.Api.Models;
using MazeGame.Api.Contracts;
using MazeGameAi.src.PathFinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace MazeGameAi.src.Agents
{
    // This is agent is designed to move away from the opponent, uses Dijkstra's algorithm to find the best path away from the opponent.
    public class FugalAgent : IAgent 
    {
        public Direction decideMove(PollResponse gameState)
        {
            Dijkstra dijkstra = new Dijkstra();
            return dijkstra.NextMoveAwayFromOpponent(gameState);
        }

    }
}
