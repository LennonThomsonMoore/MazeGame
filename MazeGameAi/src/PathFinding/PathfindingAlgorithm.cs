using MazeGame.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGameAi.src.PathFinding
{
    public interface PathfindingAlgorithm
    {
        public Direction NextMoveTowardsTarget(Cell[][] maze, PlayerPosition yourPosition, PlayerPosition? targetPosition, IEnumerable<PlayerPosition>? positionsToAvoid = null);
        public Direction NextMoveAwayFromTarget(Cell[][] maze, PlayerPosition yourPosition, PlayerPosition? targetPosition, IEnumerable<PlayerPosition>? positionsToAvoid = null);
    }
}
