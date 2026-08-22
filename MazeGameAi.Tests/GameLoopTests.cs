using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using MazeGameAi;
using MazeGameAi.src.GameLoop;
using MazeGame.Api.Services;
using Autofac.Features.Indexed;
using MazeGame.Api.Models;
using MazeGame.Api.Contracts;
using MazeGameAi.src.Client;
using MazeGameAi.src.Agents;

namespace MazeGameAi.Tests
{
    public class GameLoopTests
    {

        private readonly Mock<IAgent> _gameServiceMock = new Mock<IAgent>();
        private readonly Mock<IIndex<PlayerType, IAgent>> _agentFactoryMock = new Mock<IIndex<PlayerType, IAgent>>();
        private readonly Mock<IMazeGameApiClient> _mazeGameApiClientMock = new Mock<IMazeGameApiClient>();
        private GameLoop _gameLoop;
        public GameLoopTests()
        {
            _agentFactoryMock.Setup(x => x[It.IsAny<PlayerType>()]).Returns(_gameServiceMock.Object);
            _gameLoop = new GameLoop(_agentFactoryMock.Object, _mazeGameApiClientMock.Object);
        }
        [Fact]
        public async Task NullGameIdCausesCreateGameToBeCalled()
        {
            var gameId = Guid.NewGuid();
            var playerToken = Guid.NewGuid();

            _mazeGameApiClientMock
                .Setup(x => x.CreateGameAsync())
                .ReturnsAsync(new CreateGameResponse(gameId, playerToken, PlayerType.Hider, GameStatus.WaitingForPlayer));

            _mazeGameApiClientMock
                .SetupSequence(x => x.PollAsync(playerToken, gameId))
                .ReturnsAsync(new PollResponse(gameId, null, null, PlayerType.Hider, 0, 0, GameStatus.Active, null, null))
                .ReturnsAsync(new PollResponse(gameId, null, null, PlayerType.Hider, 0, 0, GameStatus.Completed, PlayerType.Hider, null));

            await _gameLoop.Start(null);

            _mazeGameApiClientMock.Verify(x => x.CreateGameAsync(), Times.Once);
            _mazeGameApiClientMock.Verify(x => x.JoinGameAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task SetGameIdCausesJoinGameToBeCalled()
        {
            var gameId = Guid.NewGuid();
            var playerToken = Guid.NewGuid();

            _mazeGameApiClientMock
                .Setup(x => x.JoinGameAsync(gameId))
                .ReturnsAsync(new JoinGameResponse(gameId, playerToken, PlayerType.Seeker, GameStatus.WaitingForPlayer));

            _mazeGameApiClientMock
                .SetupSequence(x => x.PollAsync(playerToken, gameId))
                .ReturnsAsync(new PollResponse(gameId, null, null, PlayerType.Seeker, 0, 0, GameStatus.Active, null, null))
                .ReturnsAsync(new PollResponse(gameId, null, null, PlayerType.Seeker, 0, 0, GameStatus.Completed, PlayerType.Seeker, null));

            await _gameLoop.Start(gameId);

            _mazeGameApiClientMock.Verify(x => x.JoinGameAsync(gameId), Times.Once);
            _mazeGameApiClientMock.Verify(x => x.CreateGameAsync(), Times.Never);
        }

        [Fact]
        public async Task AIMovesIfGameIsActive()
        {
            var gameId = Guid.NewGuid();
            var playerToken = Guid.NewGuid();
            var direction = Direction.North;
            var maze = new Cell[][] { new[] { Cell.Empty } };
            var position = new PlayerPosition(0, 0);

            _mazeGameApiClientMock
                .Setup(x => x.CreateGameAsync())
                .ReturnsAsync(new CreateGameResponse(gameId, playerToken, PlayerType.Hider, GameStatus.WaitingForPlayer));

            _mazeGameApiClientMock
                .SetupSequence(x => x.PollAsync(playerToken, gameId))
                .ReturnsAsync(new PollResponse(gameId, null, null, PlayerType.Hider, 0, 0, GameStatus.Active, null, null))
                .ReturnsAsync(new PollResponse(gameId, position, null, PlayerType.Hider, 0, 0, GameStatus.Active, null, maze))
                .ReturnsAsync(new PollResponse(gameId, null, null, PlayerType.Hider, 0, 0, GameStatus.Completed, PlayerType.Hider, null));

            _gameServiceMock
                .Setup(x => x.decideMove(It.IsAny<PollResponse>()))
                .Returns(direction);

            _mazeGameApiClientMock
                .Setup(x => x.MoveAsync(playerToken, gameId, direction))
                .ReturnsAsync(MoveResult.Success(null));

            await _gameLoop.Start(null);

            _mazeGameApiClientMock.Verify(x => x.MoveAsync(playerToken, gameId, direction), Times.Once);
        }
    }
}
