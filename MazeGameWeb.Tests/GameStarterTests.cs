using MazeGame.Api.Models;
using MazeGame.Api.Services;

namespace MazeGameWeb.Tests;

public class GameStarterTests
{
    private class FakeMazeGenerator : IMazeGenerator
    {
        private readonly Cell[][] _maze;

        public FakeMazeGenerator(Cell[][] maze)
        {
            _maze = maze;
        }

        public Cell[][] Generate() => _maze;
    }

    private static Cell[][] CreateAllEmptyMaze(int size)
    {
        var maze = new Cell[size][];
        for (int x = 0; x < size; x++)
        {
            maze[x] = new Cell[size];
            for (int y = 0; y < size; y++)
            {
                maze[x][y] = Cell.Empty;
            }
        }
        return maze;
    }

    [Fact]
    public void Start_SetsGameStatusToActive()
    {
        var game = Game.CreateHiderGame(Guid.NewGuid(), null);
        var generator = new FakeMazeGenerator(CreateAllEmptyMaze(5));

        var result = GameStarter.Start(game, generator);

        Assert.Equal(GameStatus.Active, result.GameStatus);
    }

    [Fact]
    public void Start_SetsMazeFromGenerator()
    {
        var game = Game.CreateHiderGame(Guid.NewGuid(), null);
        var maze = CreateAllEmptyMaze(5);
        var generator = new FakeMazeGenerator(maze);

        var result = GameStarter.Start(game, generator);

        Assert.Same(maze, result.Maze);
    }

    [Fact]
    public void Start_UpdatesUpdatedAtTimestamp()
    {
        var game = Game.CreateHiderGame(Guid.NewGuid(), null);
        var before = game.UpdatedAt;
        var generator = new FakeMazeGenerator(CreateAllEmptyMaze(5));

        var result = GameStarter.Start(game, generator);

        Assert.True(result.UpdatedAt >= before);
    }

    [Fact]
    public void Start_PlacesHiderAndSeekerOnEmptyCells()
    {
        var game = Game.CreateHiderGame(Guid.NewGuid(), null);
        var maze = CreateAllEmptyMaze(5);
        var generator = new FakeMazeGenerator(maze);

        var result = GameStarter.Start(game, generator);

        Assert.NotNull(result.HiderPosition);
        Assert.NotNull(result.SeekerPosition);
        Assert.Equal(Cell.Empty, maze[result.HiderPosition!.Row][result.HiderPosition.Column]);
        Assert.Equal(Cell.Empty, maze[result.SeekerPosition!.Row][result.SeekerPosition.Column]);
    }

    [Fact]
    public void Start_PlacesHiderAndSeekerNotAdjacentOrDiagonal()
    {
        var game = Game.CreateHiderGame(Guid.NewGuid(), null);
        var maze = CreateAllEmptyMaze(5);
        var generator = new FakeMazeGenerator(maze);

        var result = GameStarter.Start(game, generator);

        int rowDiff = Math.Abs(result.HiderPosition!.Row - result.SeekerPosition!.Row);
        int colDiff = Math.Abs(result.HiderPosition.Column - result.SeekerPosition.Column);
        Assert.True(rowDiff > 1 || colDiff > 1);
    }

    [Fact]
    public void Start_HiderAndSeekerAreAtDifferentPositions()
    {
        var game = Game.CreateHiderGame(Guid.NewGuid(), null);
        var generator = new FakeMazeGenerator(CreateAllEmptyMaze(5));

        var result = GameStarter.Start(game, generator);

        Assert.False(result.HiderPosition!.Equals(result.SeekerPosition));
    }

    [Fact]
    public void Start_ReturnsSameGameInstance()
    {
        var game = Game.CreateHiderGame(Guid.NewGuid(), null);
        var generator = new FakeMazeGenerator(CreateAllEmptyMaze(5));

        var result = GameStarter.Start(game, generator);

        Assert.Same(game, result);
    }
}
