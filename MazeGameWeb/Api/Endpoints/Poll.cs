using MazeGame.Api.Contracts;
using MazeGame.Api.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using MazeGame.Api.Models;

namespace MazeGame.Api.Endpoints
{
    public static class Poll
    {
        private const int RevealTurnInterval = 5;
        public static void MapPollEndpoint(this WebApplication app)
        {
            app.MapGet("/poll", ([FromQuery] Guid? playerToken, [FromQuery] Guid? gameId, [FromServices] GameDbContext db) => {

                if (playerToken == null || gameId == null)
                {
                    return Results.BadRequest("playerToken and gameId query parameters are required.");
                }

                var game = db.Games.FirstOrDefault(g => g.GameId == gameId);
                if (game == null)
                {
                    return Results.NotFound($"Game with id {gameId} not found.");
                }
                if (game.HiderToken != playerToken && game.SeekerToken != playerToken)
                {
                    return Results.BadRequest("CurrentPlayer must be either Hider or Seeker.");
                }
                bool isHider = (game.HiderToken == playerToken);

                if (game.GameStatus == GameStatus.WaitingForPlayer)
                { 
                    var response = PollResponse.ForWaitingForPlayer(game.GameId, game.GameStatus);
                    return Results.Ok(response);
                }

                if (game.GameStatus == GameStatus.Completed)
                {
                    if (game.Winner == null)
                    {
                        return Results.Problem("Winner should not be null when game is completed.");
                    }
                    var response = PollResponse.ForGameOver(game.GameId, game.GameStatus, game.Winner.Value);
                    return Results.Ok(response);
                }

                //#### Visibility Rules
                //1.A player always sees their own position.  
                //2.Opponent position is hidden during normal play.  
                //3.Opponent position is returned only on reveal turns.
                //4.Reveal turns occur at the end of turn which is a multiple of 5.

                if (game.TurnNumber % RevealTurnInterval == 0 && game.TurnNumber != 0)
                {
                    //Reveal turn
                    var response = new PollResponse
                    (
                        game.GameId,
                        isHider ? game.HiderPosition : game.SeekerPosition,
                        isHider ? game.SeekerPosition : game.HiderPosition,
                        game.CurrentPlayer,
                        game.TurnNumber,
                        RevealTurnInterval - game.TurnNumber % RevealTurnInterval,
                        game.GameStatus,
                        game.Winner,
                        game.Maze
                    );
                    return Results.Ok(response);
                }
                else
                {
                    //Normal turn
                    var response = new PollResponse
                    (
                        game.GameId,
                        isHider ? game.HiderPosition : game.SeekerPosition,
                        null, //Opponent position is hidden during normal play
                        game.CurrentPlayer,
                        game.TurnNumber,
                        RevealTurnInterval - game.TurnNumber % RevealTurnInterval,
                        game.GameStatus,
                        game.Winner,
                        game.Maze
                    );
                    return Results.Ok(response);
                }

            });
        }
    }
}
