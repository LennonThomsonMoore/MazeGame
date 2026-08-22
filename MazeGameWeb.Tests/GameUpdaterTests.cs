using MazeGame.Api.Models;
using MazeGame.Api.Services;

namespace MazeGameWeb.Tests;

public class GameUpdaterTests
{
    private static Game CreateGame(PlayerPosition hiderPosition, PlayerPosition seekerPosition, PlayerType currentPlayer = PlayerType.Seeker, int turnNumber = 0)
    {
        var game = Game.CreateHiderGame(Guid.NewGuid(), null);
        game.HiderPosition = hiderPosition;
        game.SeekerPosition = seekerPosition;
        game.CurrentPlayer = currentPlayer;
        game.TurnNumber = turnNumber;
        game.GameStatus = GameStatus.Active;
        return game;
    }

    [Theory]
    [InlineData(Direction.North, 4, 5)]
    [InlineData(Direction.South, 6, 5)]
    [InlineData(Direction.West, 5, 4)]
    [InlineData(Direction.East, 5, 6)]
    public void Update_MovesHiderInExpectedDirection(Direction direction, int expectedRow, int expectedColumn)
    {
        var game = CreateGame(new PlayerPosition(5, 5), new PlayerPosition(0, 0));

        GameUpdater.update(direction, PlayerType.Hider, game);

        Assert.Equal(expectedRow, game.HiderPosition!.Row);
        Assert.Equal(expectedColumn, game.HiderPosition.Column);
    }

    [Theory]
    [InlineData(Direction.North, 4, 5)]
    [InlineData(Direction.South, 6, 5)]
    [InlineData(Direction.West, 5, 4)]
    [InlineData(Direction.East, 5, 6)]
    public void Update_MovesSeekerInExpectedDirection(Direction direction, int expectedRow, int expectedColumn)
    {
        var game = CreateGame(new PlayerPosition(0, 0), new PlayerPosition(5, 5));

        GameUpdater.update(direction, PlayerType.Seeker, game);

        Assert.Equal(expectedRow, game.SeekerPosition!.Row);
        Assert.Equal(expectedColumn, game.SeekerPosition.Column);
    }

    [Fact]
    public void Update_DoesNotMoveOtherPlayer()
    {
        var seekerStart = new PlayerPosition(0, 0);
        var game = CreateGame(new PlayerPosition(5, 5), seekerStart);

        GameUpdater.update(Direction.North, PlayerType.Hider, game);

        Assert.Equal(seekerStart.Row, game.SeekerPosition!.Row);
        Assert.Equal(seekerStart.Column, game.SeekerPosition.Column);
    }

    [Fact]
    public void Update_WhenSeekerMovesOntoHider_MarksGameCompletedWithSeekerWinner()
    {
        var game = CreateGame(new PlayerPosition(4, 5), new PlayerPosition(5, 5));

        GameUpdater.update(Direction.North, PlayerType.Seeker, game);

        Assert.Equal(GameStatus.Completed, game.GameStatus);
        Assert.Equal(PlayerType.Seeker, game.Winner);
    }

    [Fact]
    public void Update_WhenHiderMovesOntoSeeker_MarksGameCompletedWithSeekerWinner()
    {
        var game = CreateGame(new PlayerPosition(5, 5), new PlayerPosition(4, 5));

        GameUpdater.update(Direction.North, PlayerType.Hider, game);

        Assert.Equal(GameStatus.Completed, game.GameStatus);
        Assert.Equal(PlayerType.Seeker, game.Winner);
    }

    [Fact]
    public void Update_WhenNoCapture_SwitchesCurrentPlayerFromHiderToSeeker()
    {
        var game = CreateGame(new PlayerPosition(5, 5), new PlayerPosition(0, 0), currentPlayer: PlayerType.Hider);

        GameUpdater.update(Direction.North, PlayerType.Hider, game);

        Assert.Equal(PlayerType.Seeker, game.CurrentPlayer);
    }

    [Fact]
    public void Update_WhenNoCapture_SwitchesCurrentPlayerFromSeekerToHider()
    {
        var game = CreateGame(new PlayerPosition(5, 5), new PlayerPosition(0, 0), currentPlayer: PlayerType.Seeker);

        GameUpdater.update(Direction.North, PlayerType.Seeker, game);

        Assert.Equal(PlayerType.Hider, game.CurrentPlayer);
    }

    [Fact]
    public void Update_WhenSeekerMoves_IncrementsTurnNumber()
    {
        var game = CreateGame(new PlayerPosition(5, 5), new PlayerPosition(0, 0), turnNumber: 3);

        GameUpdater.update(Direction.North, PlayerType.Seeker, game);

        Assert.Equal(4, game.TurnNumber);
    }

    [Fact]
    public void Update_WhenHiderMoves_DoesNotIncrementTurnNumber()
    {
        var game = CreateGame(new PlayerPosition(5, 5), new PlayerPosition(0, 0), turnNumber: 3);

        GameUpdater.update(Direction.North, PlayerType.Hider, game);

        Assert.Equal(3, game.TurnNumber);
    }

    [Fact]
    public void Update_WhenTurnNumberReaches100AfterSeekerMove_MarksGameCompletedWithHiderWinner()
    {
        var game = CreateGame(new PlayerPosition(10, 10), new PlayerPosition(0, 0), turnNumber: 99);

        GameUpdater.update(Direction.North, PlayerType.Seeker, game);

        Assert.Equal(GameStatus.Completed, game.GameStatus);
        Assert.Equal(PlayerType.Hider, game.Winner);
    }

    [Fact]
    public void Update_WhenTurnNumberBelow100AfterSeekerMove_GameRemainsActive()
    {
        var game = CreateGame(new PlayerPosition(10, 10), new PlayerPosition(0, 0), turnNumber: 5);

        GameUpdater.update(Direction.North, PlayerType.Seeker, game);

        Assert.Equal(GameStatus.Active, game.GameStatus);
        Assert.Null(game.Winner);
    }

    [Fact]
    public void Update_SetsUpdatedAtTimestamp()
    {
        var game = CreateGame(new PlayerPosition(5, 5), new PlayerPosition(0, 0));
        var before = game.UpdatedAt;

        GameUpdater.update(Direction.North, PlayerType.Hider, game);

        Assert.True(game.UpdatedAt >= before);
    }
}
