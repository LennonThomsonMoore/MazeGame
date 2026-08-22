using MazeGame.Api.Models;
using MazeGame.Api.Services;

namespace MazeGameWeb.Tests;

public class CellularAutomataMazeGeneratorTests
{
    [Fact]
    public void Generate_ReturnsMazeWithExpectedDimensions()
    {
        var generator = new CellularAutomataMazeGenerator();

        Cell[][] maze = generator.Generate();

        Assert.Equal(Maze.size, maze.Length);
        Assert.All(maze, row => Assert.Equal(Maze.size, row.Length));
    }

    [Fact]
    public void Generate_OpensStartAndEndCorners()
    {
        var generator = new CellularAutomataMazeGenerator();

        Cell[][] maze = generator.Generate();

        Assert.Equal(Cell.Empty, maze[Maze.FirstIndex][Maze.FirstIndex]);
        Assert.Equal(Cell.Empty, maze[Maze.LastIndex][Maze.LastIndex]);
        Assert.Equal(Cell.Empty, maze[Maze.LastIndex - 1][Maze.LastIndex]);
        Assert.Equal(Cell.Empty, maze[Maze.LastIndex][Maze.LastIndex - 1]);
    }

    [Fact]
    public void Generate_ProducesFullyConnectedMaze()
    {
        var generator = new CellularAutomataMazeGenerator();

        Cell[][] maze = generator.Generate();

        Assert.True(MazeGeneratorTestHelpers.IsFullyConnected(maze));
    }

    [Fact]
    public void Generate_CalledMultipleTimes_AlwaysProducesConnectedMaze()
    {
        var generator = new CellularAutomataMazeGenerator();

        for (int i = 0; i < 5; i++)
        {
            Cell[][] maze = generator.Generate();
            Assert.True(MazeGeneratorTestHelpers.IsFullyConnected(maze));
        }
    }
}
