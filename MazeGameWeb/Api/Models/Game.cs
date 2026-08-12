using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace MazeGame.Api.Models
{
    public class Game
    {
        [Key]
        public Guid GameId { get; set; }
        public GameStatus GameStatus { get; set; }
        public Cell[][]? Maze { get; set; }
        public PlayerPosition? HiderPosition { get; set; }
        public PlayerPosition? SeekerPosition { get; set; }
        public PlayerType CurrentPlayer { get; set; }
        public int TurnNumber { get; set; }
        public PlayerType? Winner { get; set; }
        public Guid? HiderToken { get; set; }
        public Guid? SeekerToken { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        private Game() { }
        private Game(
            Guid GameId,
            GameStatus GameStatus,
            Cell[][]? Maze,
            PlayerPosition? HiderPosition,
            PlayerPosition? SeekerPosition,
            PlayerType CurrentPlayer,
            int TurnNumber,
            PlayerType? Winner,
            Guid? HiderToken,
            Guid? SeekerToken,
            DateTimeOffset CreatedAt,
            DateTimeOffset UpdatedAt
        )
        {
            this.GameId = GameId;
            this.GameStatus = GameStatus;
            this.Maze = Maze;
            this.HiderPosition = HiderPosition;
            this.SeekerPosition = SeekerPosition;
            this.CurrentPlayer = CurrentPlayer;
            this.TurnNumber = TurnNumber;
            this.Winner = Winner;
            this.HiderToken = HiderToken;
            this.SeekerToken = SeekerToken;
            this.CreatedAt = CreatedAt;
            this.UpdatedAt = UpdatedAt;
        } 

        public static Game CreateHiderGame(Guid HostToken)
        {
            return new Game(
                Guid.NewGuid(),
                GameStatus.WaitingForPlayer,
                null,
                null,
                null,
                PlayerType.Hider,
                0,
                null,
                HostToken,
                null,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow
            );
            
        }


        public static Game CreateSeekerGame(Guid HostToken)
        {
            return new Game(
                Guid.NewGuid(),
                GameStatus.WaitingForPlayer,
                null,
                null,
                null,
                PlayerType.Hider,
                0,
                null,
                null,
                HostToken,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow
            );
            
        }
    }

 
}
