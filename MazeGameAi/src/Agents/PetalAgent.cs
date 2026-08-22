using MazeGame.Api.Models;
using MazeGame.Api.Contracts;
using MazeGameAi.src.PathFinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace MazeGameAi.src.Agents
{
    // This is agent is designed to move towards the opponent, uses Dijkstra's algorithm to find the best path towards the opponent.
    public class PetalAgent : IAgent
    {
        private readonly PathfindingAlgorithm _pathfindingAlgorithm;
        public PetalAgent(PathfindingAlgorithm pathfindingAlgorithm)
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
            return _pathfindingAlgorithm.NextMoveTowardsTarget(gameState.Maze, gameState.YourPosition, lastSeenOpponentPosition);
        }

    }
}
