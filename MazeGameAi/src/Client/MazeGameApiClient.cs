using System.Net;
using System.Net.Http.Json;
using MazeGame.Api.Contracts;
using MazeGame.Api.Models;

namespace MazeGameAi.src.Client
{

	public class MazeGameApiClient : IMazeGameApiClient
    {
		private const string BaseUrl = "http://localhost:5292";
		private static readonly CookieContainer cookieContainer = new();
		private static readonly HttpClientHandler handler = new() { CookieContainer = cookieContainer };
		private static HttpClient client = new(handler) { BaseAddress = new Uri(BaseUrl) };

		private static bool _isAuthenticated;
		private static readonly SemaphoreSlim authLock = new(1, 1);

		/// <summary>
		/// Ensures the AI has an authenticated session before making game requests.
		/// Registers a new, randomly named account for this AI instance on first use,
		/// storing the resulting auth cookie in the shared CookieContainer for all
		/// future calls. On the rare chance the generated username already exists,
		/// a new random username/password pair is generated and registration is retried.
		/// </summary>
		private async Task EnsureAuthenticatedAsync()
		{
			if (_isAuthenticated)
			{
				return;
			}

			await authLock.WaitAsync();
			try
			{
				if (_isAuthenticated)
				{
					return;
				}

				const int maxAttempts = 5;
				for (var attempt = 1; attempt <= maxAttempts; attempt++)
				{
					var username = $"ai-{Guid.NewGuid():N}";
					var password = Guid.NewGuid().ToString("N");

					var registerRequest = new RegisterRequest(username, password);
					var registerResponse = await client.PostAsJsonAsync("/auth/register", registerRequest);

					if (registerResponse.StatusCode == HttpStatusCode.Conflict)
					{
						continue;
					}

					registerResponse.EnsureSuccessStatusCode();
					_isAuthenticated = true;
					return;
				}

				throw new InvalidOperationException($"Failed to register an AI account after {maxAttempts} attempts due to repeated username conflicts.");
			}
			finally
			{
				authLock.Release();
			}
		}

		public async Task<PollResponse> PollAsync(Guid playerToken, Guid gameId)
		{
			await EnsureAuthenticatedAsync();
			var response = await client.GetAsync($"/poll?playerToken={playerToken}&gameId={gameId}");
            response.EnsureSuccessStatusCode();
            var pollResponse = await response.Content.ReadFromJsonAsync<PollResponse>();
            return pollResponse;
        }

        public async Task<MoveResult> MoveAsync(Guid playerToken, Guid gameId, Direction direction)
        {
            await EnsureAuthenticatedAsync();
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

        public async Task<CreateGameResponse> CreateGameAsync()
        {
            await EnsureAuthenticatedAsync();
            var response = await client.PostAsync("/create", null);
            response.EnsureSuccessStatusCode();
            var createGameResponse = await response.Content.ReadFromJsonAsync<CreateGameResponse>();
            return createGameResponse;
        }

        public async Task<JoinGameResponse> JoinGameAsync(Guid gameId)
        {
            await EnsureAuthenticatedAsync();
            var joinGameRequest = new JoinGameRequest(gameId);
            var response = await client.PostAsJsonAsync("/join", joinGameRequest);
            response.EnsureSuccessStatusCode();
            var joinGameResponse = await response.Content.ReadFromJsonAsync<JoinGameResponse>();
            return joinGameResponse;
        } 

    }

}


