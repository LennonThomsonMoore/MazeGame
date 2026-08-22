using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MazeGame.Api.Models;
using MazeGame.Api.Contracts;
using System.Diagnostics;

namespace MazeGameAi.src.PathFinding
{
    public class Dijkstra : PathfindingAlgorithm
    {
        private readonly IRandomDirectionGenerator _generator;

        public Dijkstra(IRandomDirectionGenerator generator) 
        { 
            _generator = generator;
        }

        // Performs Dijkstra's algorithm to find the shortest path from the player's position toward the target's position in the maze.
        public Direction NextMoveTowardsTarget(Cell[][] maze, PlayerPosition yourPosition, PlayerPosition? targetPosition, IEnumerable<PlayerPosition>? positionsToAvoid = null)
        {
            return NextMove(towardsOpponent: true, maze, yourPosition, targetPosition, positionsToAvoid);
        }

        // Performs Dijkstra's algorithm to find the shortest path from the player's position away from the target's position in the maze.
        public Direction NextMoveAwayFromTarget(Cell[][] maze, PlayerPosition yourPosition, PlayerPosition? targetPosition, IEnumerable<PlayerPosition>? positionsToAvoid = null)
        {
            return NextMove(towardsOpponent: false, maze, yourPosition, targetPosition, positionsToAvoid);
        }

        private Direction NextMove(bool towardsOpponent, Cell[][] maze, PlayerPosition yourPosition, PlayerPosition? targetPosition, IEnumerable<PlayerPosition>? positionsToAvoid)
        {
            Console.WriteLine("Calculating next move using Dijkstra's algorithm...");
            if (maze == null) throw new ArgumentNullException(nameof(maze), "Maze is null");
            if (yourPosition == null) throw new ArgumentNullException(nameof(yourPosition), "Your position is null");
            //if not target pick random direction
            if (targetPosition == null)
            {
                return _generator.generate();
            }

            HashSet<(int Row, int Col)> avoidSet = positionsToAvoid?
                .Select(p => (p.Row, p.Column))
                .ToHashSet() ?? new HashSet<(int, int)>();

            bool IsPassable(int row, int col)
            {
                return row >= 0 && row < maze.Length
                    && col >= 0 && col < maze[0].Length
                    && maze[row][col] != Cell.Wall
                    && !avoidSet.Contains((row, col));
            }

            int[][] distance = new int[maze.Length][];
            for (int i = 0; i < maze.Length; i++)
            {
                distance[i] = new int[maze[0].Length];
                for (int j = 0; j < maze[0].Length; j++)
                {
                    distance[i][j] = int.MaxValue;
                }
            }

            Queue<(int, int)> queue = new Queue<(int, int)>();
            queue.Enqueue((targetPosition.Row, targetPosition.Column));
            distance[targetPosition.Row][targetPosition.Column] = 0;


            int[] dRow = { -1, 1, 0, 0 };
            int[] dCol = { 0, 0, -1, 1 };

            while (queue.Count > 0)
            {
                var (row, col) = queue.Dequeue();
                for (int i = 0; i < 4; i++)
                {
                    int newRow = row + dRow[i];
                    int newCol = col + dCol[i];
                    if (IsPassable(newRow, newCol))
                    {
                        int newDist = distance[row][col] + 1;
                        if (newDist < distance[newRow][newCol])
                        {
                            distance[newRow][newCol] = newDist;
                            queue.Enqueue((newRow, newCol));
                        }
                    }
                }
            }
            
            // Determine the next move that most reduces/increases the distance to the opponent
            int minDistance = (towardsOpponent ? int.MaxValue : int.MinValue);
            Direction? nextMove = null;
            for (int i = 0; i < 4; i++)
            {
                int newRow = yourPosition.Row + dRow[i];
                int newCol = yourPosition.Column + dCol[i];
                if (IsPassable(newRow, newCol))
                {
                    int neighborDistance = distance[newRow][newCol];
                    if (towardsOpponent)
                    {
                        if (neighborDistance != int.MaxValue && neighborDistance < minDistance)
                        {
                            minDistance = neighborDistance;
                            nextMove = (Direction)i;
                        }
                    }
                    else
                    {
                        if (neighborDistance != int.MaxValue && neighborDistance > minDistance)
                        {
                            minDistance = neighborDistance;
                            nextMove = (Direction)i;
                        }
                    }
                }
            }

            Direction resolvedMove = nextMove ?? _generator.generate();
            Console.WriteLine($"Calculated next move using Dijkstra's algorithm. {resolvedMove} with {minDistance} distance. ");
            return resolvedMove;
        }
    }
}
