using MazeGameAi.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MazeGame.Api.Models;
using MazeGameAi.src.Agents;

namespace MazeGameAi.src.GameLoop
{
    public class GameLoop
    {
        private Guid _GameId;
        private Guid _PlayerToken;
        private PlayerType _Role;

        public TimeSpan PollRate { get; set; } = TimeSpan.FromSeconds(1);

        private IAgent agent;


        public async Task Start(Guid? GameId = null)
        {
            if (GameId == null)
            {
                Console.WriteLine("Creating game... ");
                var response = await MazeGameApiClient.CreateGameAsync();
                Console.WriteLine($"Created game at {response.gameId} ");
                _GameId = response.gameId;
                _PlayerToken = response.playerToken;
                _Role = response.role;
            }
            else
            {
                Console.WriteLine($"Joining game at {GameId}");
                var response = await MazeGameApiClient.JoinGameAsync(GameId.Value);
                _GameId = GameId.Value;
                _PlayerToken = response.playerToken;
                _Role = response.role;
            }

            Console.WriteLine("I am a " + _Role.ToString());

            //Decides strategy

            if (_Role == PlayerType.Hider)
            {
                agent = new ClustersAgent();
            }
            else
            {
                agent = new PetalAgent();
            }

            //Wait for game to start

            Boolean started = false;
            while (!started)
            {
                var response = await MazeGameApiClient.PollAsync(_PlayerToken, _GameId);
                if (response.Status == GameStatus.Active)
                {
                    started = true;
                    break;
                }
                await Task.Delay(PollRate);
            }

            //Polls server and performs move
            while (true)
            {
                var response = await MazeGameApiClient.PollAsync(_PlayerToken, _GameId);
                if (response.Status == GameStatus.Completed)
                {
                    Console.WriteLine("The game has ended. " + response.Winner.ToString() + " won.");
                    return;
                }

                if (response.CurrentPlayer == _Role)
                {
                    Console.WriteLine("I am a " + _Role.ToString());
                    DrawGameState(response);
                    Direction direction = agent.decideMove(response);
                    var response2 = await MazeGameApiClient.MoveAsync(_PlayerToken, _GameId, direction);
                    if (response2.IsSuccess)
                    {
                        Console.WriteLine($"Moved {direction} successfully.");
                        await Task.Delay(PollRate);
                    }
                    else
                    {
                        Console.WriteLine($"Failed to move {direction}. {response2.Error}");
                    }

                }
            }
        }

        private static void DrawGameState(MazeGame.Api.Contracts.PollResponse gameState)
        {
            Cell[][] maze = gameState.Maze;
            PlayerPosition yourPosition = gameState.YourPosition;
            PlayerPosition? opponentPosition = gameState.OpponentPosition;
            ConsoleColor originalColor = Console.ForegroundColor;

            for (int row = 0; row < maze.Length; row++)
            {
                for (int col = 0; col < maze[row].Length; col++)
                {
                    bool isYou = row == yourPosition.Row && col == yourPosition.Column;
                    bool isOpponent = opponentPosition != null && row == opponentPosition.Row && col == opponentPosition.Column;

                    if (isYou)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write("█");
                    }
                    else if (isOpponent)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("█");
                    }
                    else if (maze[row][col] == Cell.Wall)
                    {
                        Console.ForegroundColor = originalColor;
                        Console.Write("█");
                    }
                    else
                    {
                        Console.ForegroundColor = originalColor;
                        Console.Write("□");
                    }
                }

                Console.ForegroundColor = originalColor;
                Console.WriteLine();
            }

            Console.ForegroundColor = originalColor;
        }
    }
}
