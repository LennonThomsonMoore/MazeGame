namespace MazeGame.Api.Endpoints
{
    public static class Logout
    {
        public static void MapLogoutEndpoint(this WebApplication app)
        {
            app.MapPost("/auth/logout", (HttpContext httpContext) =>
            {
                httpContext.Response.Cookies.Delete(Login.AuthCookieName);
                return Results.Ok();
            }).RequireAuthorization();
        }
    }
}
