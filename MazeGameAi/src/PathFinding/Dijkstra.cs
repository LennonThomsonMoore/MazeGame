using System;
using System.Collections.Generic;
using System.Text;
using MazeGame.Api.Models;
using MazeGame.Api.Contracts;
using System.Diagnostics;

namespace MazeGameAi.src.PathFinding
{
    public class Dijkstra
    {

        private static PlayerPosition? targetPosition = null;

        //Performs Dijkstra's algorithm to find the shortest path from the player's position to the target position in the maze.
        public Direction NextMove(bool towardsOpponent, PollResponse gameState)
        {
            Console.WriteLine("Calculating next move using Dijkstra's algorithm...");
            Debug.Assert(gameState.Maze != null, "Maze is null");
            Cell[][] maze = gameState.Maze;
            PlayerPosition yourPosition = gameState.YourPosition;
            if (targetPosition == null)
            {
                targetPosition = yourPosition;
            }
            if (gameState.OpponentPosition != null)
            {
                targetPosition = gameState.OpponentPosition;
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
                    if (newRow >= 0 && newRow < maze.Length && newCol >= 0 && newCol < maze[0].Length && maze[newRow][newCol] != Cell.Wall)
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
                if (newRow >= 0 && newRow < maze.Length && newCol >= 0 && newCol < maze[0].Length && maze[newRow][newCol] != Cell.Wall)
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

            Direction resolvedMove = nextMove ?? Direction.North;
            Console.WriteLine($"Calculated next move using Dijkstra's algorithm. {resolvedMove} with {minDistance} distance. ");
            return resolvedMove;
        }
    }
}
