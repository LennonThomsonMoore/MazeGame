using MazeGame.Api.Models;
using MazeGame.Api.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using MazeGameAi.src.PathFinding;

namespace MazeGameAi.src.Agents
{
	// This agent partitions the maze into 3 clusters using a simple k-means style
	// clustering algorithm. It then heads towards the closest cluster point that is
	// not in the same cluster as the opponent, moving to another such cluster once it
	// arrives (or once the opponent's cluster changes).
	public class ClustersAgent : IAgent
	{
		private const int ClusterCount = 3;
		private const int KMeansIterations = 10;

		private bool _initialized = false;
		private List<PlayerPosition> _clusterCenters = new List<PlayerPosition>();
		private int[][]? _clusterMap = null;
		private PlayerPosition? lastSeenOpponentPosition;
		private int _currentOpponentCluster = -1;
		private int _previousOpponentCluster = -1;

		public Direction decideMove(PollResponse gameState)
		{
			if (gameState.Maze == null || gameState.YourPosition == null)
			{
				return (Direction)new Random().Next(0, 4);
			}

			Cell[][] maze = gameState.Maze;

			if (!_initialized)
			{
				GenerateClusters(maze);
				_initialized = true;
			}

			PlayerPosition yourPosition = gameState.YourPosition;
			PlayerPosition? opponentPosition = gameState.OpponentPosition;
			if (opponentPosition != null)
			{
				lastSeenOpponentPosition = opponentPosition;
			}

			if (opponentPosition == null && lastSeenOpponentPosition == null)
			{
				return (Direction)new Random().Next(0, 4);
			}

			int opponentCluster = _clusterMap![lastSeenOpponentPosition!.Row][lastSeenOpponentPosition.Column];

			// Track cluster transitions for the opponent so we know which cluster
			// they were in immediately before entering their current cluster.
			if (opponentCluster != _currentOpponentCluster)
			{
				_previousOpponentCluster = _currentOpponentCluster;
				_currentOpponentCluster = opponentCluster;
			}

			// Compute path distances from your position to every cell in the maze.
			int[][] distances = BreadthFirstSearch(maze, yourPosition);

			// Find the closest cluster center that isn't the opponent's current cluster
			// or the cluster the opponent was in just before that.
			PlayerPosition? targetCenter = null;
			int bestDistance = int.MaxValue;
			for (int i = 0; i < _clusterCenters.Count; i++)
			{
				if (i == _currentOpponentCluster || i == _previousOpponentCluster)
				{
					continue;
				}

				PlayerPosition center = _clusterCenters[i];
				int distance = distances[center.Row][center.Column];
				if (distance != int.MaxValue && distance < bestDistance)
				{
					bestDistance = distance;
					targetCenter = center;
				}
			}

			if (targetCenter == null)
			{
				// Every cluster is the opponent's cluster (or unreachable), fall back to any center.
				targetCenter = _clusterCenters.FirstOrDefault() ?? yourPosition;
			}

            Console.WriteLine("I am in cluster " + _clusterMap?[gameState.YourPosition.Row][gameState.YourPosition.Column]);
			if (_currentOpponentCluster != -1)
			{
				Console.WriteLine("Opponent is in cluster " + _currentOpponentCluster);
			}
			Console.WriteLine("Moving towards cluster " + _clusterMap?[targetCenter.Row][targetCenter.Column]);

			List<PlayerPosition> positionsToAvoid = GetSurroundingPositions(maze, lastSeenOpponentPosition!, 1);

			return Dijkstra.NextMoveTowardsTarget(maze, yourPosition, targetCenter, positionsToAvoid);
		}

		// Returns every position within the given range (a square, using Chebyshev distance)
		// of the source position that is on the maze, used to identify cells near the opponent.
		private static List<PlayerPosition> GetSurroundingPositions(Cell[][] maze, PlayerPosition source, int range)
		{
			List<PlayerPosition> positions = new List<PlayerPosition>();
			for (int row = source.Row - range; row <= source.Row + range; row++)
			{
				if (row < 0 || row >= maze.Length)
				{
					continue;
				}
				for (int col = source.Column - range; col <= source.Column + range; col++)
				{
					if (col < 0 || col >= maze[row].Length)
					{
						continue;
					}
					positions.Add(new PlayerPosition(row, col));
				}
			}
			return positions;
		}

		// Runs a simple k-means clustering algorithm over the non-wall cells of the maze
		// and stores both the resulting cluster centers and a per-cell cluster map.
		private void GenerateClusters(Cell[][] maze)
		{
			List<PlayerPosition> emptyCells = new List<PlayerPosition>();
			for (int row = 0; row < maze.Length; row++)
			{
				for (int col = 0; col < maze[row].Length; col++)
				{
					if (maze[row][col] != Cell.Wall)
					{
						emptyCells.Add(new PlayerPosition(row, col));
					}
				}
			}

			Random random = new Random();
			List<PlayerPosition> centers = emptyCells
				.OrderBy(_ => random.Next())
				.Take(ClusterCount)
				.ToList();

			int[] assignments = new int[emptyCells.Count];

			for (int iteration = 0; iteration < KMeansIterations; iteration++)
			{
				// Assign each cell to its nearest cluster center (Euclidean distance).
				for (int i = 0; i < emptyCells.Count; i++)
				{
					PlayerPosition cell = emptyCells[i];
					int closestCluster = 0;
					double closestDistance = double.MaxValue;
					for (int c = 0; c < centers.Count; c++)
					{
						double distance = EuclideanDistance(cell, centers[c]);
						if (distance < closestDistance)
						{
							closestDistance = distance;
							closestCluster = c;
						}
					}
					assignments[i] = closestCluster;
				}

				// Recompute cluster centers as the average position of assigned cells,
				// snapped to the nearest actual (non-wall) cell.
				for (int c = 0; c < centers.Count; c++)
				{
					var clusterCells = emptyCells.Where((cell, i) => assignments[i] == c).ToList();
					if (clusterCells.Count == 0)
					{
						continue;
					}

					double avgRow = clusterCells.Average(cell => cell.Row);
					double avgCol = clusterCells.Average(cell => cell.Column);

					PlayerPosition closestCell = clusterCells
						.OrderBy(cell => Math.Pow(cell.Row - avgRow, 2) + Math.Pow(cell.Column - avgCol, 2))
						.First();
					centers[c] = closestCell;
				}
			}

			// Build the final per-cell cluster map based on the last assignments.
			int[][] clusterMap = new int[maze.Length][];
			for (int row = 0; row < maze.Length; row++)
			{
				clusterMap[row] = new int[maze[row].Length];
				for (int col = 0; col < maze[row].Length; col++)
				{
					clusterMap[row][col] = -1;
				}
			}

			for (int i = 0; i < emptyCells.Count; i++)
			{
				PlayerPosition cell = emptyCells[i];
				clusterMap[cell.Row][cell.Column] = assignments[i];
			}

			_clusterCenters = centers;
			_clusterMap = clusterMap;
		}

		private static double EuclideanDistance(PlayerPosition a, PlayerPosition b)
		{
			return Math.Sqrt(Math.Pow(a.Row - b.Row, 2) + Math.Pow(a.Column - b.Column, 2));
		}

		// Performs a breadth-first search from the source position, returning the
		// shortest path distance (in moves) from source to every cell in the maze.
		private static int[][] BreadthFirstSearch(Cell[][] maze, PlayerPosition source)
		{
			int[][] distance = new int[maze.Length][];
			for (int i = 0; i < maze.Length; i++)
			{
				distance[i] = new int[maze[0].Length];
				for (int j = 0; j < maze[0].Length; j++)
				{
					distance[i][j] = int.MaxValue;
				}
			}

			int[] dRow = { -1, 1, 0, 0 };
			int[] dCol = { 0, 0, -1, 1 };

			Queue<(int, int)> queue = new Queue<(int, int)>();
			queue.Enqueue((source.Row, source.Column));
			distance[source.Row][source.Column] = 0;

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

			return distance;
		}

	}
}
