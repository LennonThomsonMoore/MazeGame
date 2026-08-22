using MazeGame.Api.Contracts;

namespace MazeGameAi.src.Client
{
    /// <summary>
    /// Wraps the outcome of a move attempt so callers can distinguish
    /// a rejected/invalid move from a successful one without relying on exceptions.
    /// </summary>
    public class MoveResult
    {
        public bool IsSuccess { get; }
        public MoveResponse? Response { get; }
        public string? Error { get; }

        private MoveResult(bool isSuccess, MoveResponse? response, string? error)
        {
            IsSuccess = isSuccess;
            Response = response;
            Error = error;
        }

        public static MoveResult Success(MoveResponse? response) => new(true, response, null);

        public static MoveResult Failure(string? error) => new(false, null, error);
    }
}
