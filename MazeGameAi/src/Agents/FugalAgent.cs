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
        private readonly PathfindingAlgorithm _pathfindingAlgorithm;
        public FugalAgent(PathfindingAlgorithm pathfindingAlgorithm) 
        { 
            _pathfindingAlgorithm = pathfindingAlgorithm;
        }
        private PlayerPosition lastSeenOpponentPosition;
        public Direction decideMove(PollResponse gameState)
        {
            if (gameState.OpponentPosition != null)
            {
                lastSeenOpponentPosition = gameState.OpponentPosition;
            }
            return _pathfindingAlgorithm.NextMoveAwayFromTarget(gameState.Maze, gameState.YourPosition, lastSeenOpponentPosition);
        }

    }
}
