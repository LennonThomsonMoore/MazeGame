using System.Security.Claims;

namespace MazeGame.Api.Endpoints
{
    public static class Me
    {
        public static void MapMeEndpoint(this WebApplication app)
        {
            app.MapGet("/auth/me", (HttpContext httpContext) =>
            {
                var username = httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(username))
                {
                    return Results.Unauthorized();
                }

                return Results.Ok(new { username });
            }).RequireAuthorization();
        }
    }
}
