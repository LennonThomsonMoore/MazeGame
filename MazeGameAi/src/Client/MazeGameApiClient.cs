using System.Net.Http.Json;
using MazeGame.Api.Contracts;
using MazeGame.Api.Models;

namespace MazeGameAi.Client
{

    public static class MazeGameApiClient
    {
	    private const string BaseUrl = "http://localhost:5292";
	    private static HttpClient client = new() { BaseAddress = new Uri(BaseUrl) };

	    public static async Task<PollResponse> PollAsync(Guid playerToken, Guid gameId)
        {
            var response = await client.GetAsync($"/poll?playerToken={playerToken}&gameId={gameId}");
            response.EnsureSuccessStatusCode();
            var pollResponse = await response.Content.ReadFromJsonAsync<PollResponse>();
            return pollResponse;
        }

        public static async Task<MoveResult> MoveAsync(Guid playerToken, Guid gameId, Direction direction)
        {
            var moveRequest = new MoveRequest(gameId, playerToken, direction);
            var response = await client.PostAsJsonAsync("/move", moveRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var error = string.IsNullOrWhiteSpace(errorContent) ? response.ReasonPhrase : errorContent;
                return MoveResult.Failure(error);
            }

            var moveResponse = await response.Content.ReadFromJsonAsync<MoveResponse>();
            return MoveResult.Success(moveResponse);
        }

        public static async Task<CreateGameResponse> CreateGameAsync()
        {
            var response = await client.PostAsync("/create", null);
            response.EnsureSuccessStatusCode();
            var createGameResponse = await response.Content.ReadFromJsonAsync<CreateGameResponse>();
            return createGameResponse;
        }

        public static async Task<JoinGameResponse> JoinGameAsync(Guid gameId)
        {
            var joinGameRequest = new JoinGameRequest(gameId);
            var response = await client.PostAsJsonAsync("/join", joinGameRequest);
            response.EnsureSuccessStatusCode();
            var joinGameResponse = await response.Content.ReadFromJsonAsync<JoinGameResponse>();
            return joinGameResponse;
        } 

    }

}


