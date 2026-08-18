using FluentValidation;
using MazeGame.Api.Contracts;
using MazeGame.Api.Data;
using MazeGame.Api.Services;
using MazeGame.Api.Validators;
using Microsoft.EntityFrameworkCore;

namespace MazeGame.Api.Endpoints
{
    public static class Login
    {
        public const string AuthCookieName = "auth_token";

        public static void MapLoginEndpoints(this WebApplication app)
        {
            app.MapPost("/auth/login", async (HttpContext httpContext, UserDbContext db, LoginRequest request, IValidator<UserWithLoginRequest> validator, ITokenService tokenService) =>
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
                var userWithRequest = new UserWithLoginRequest(user, request);
                var validationResult = await validator.ValidateAsync(userWithRequest);
                if (!validationResult.IsValid)
                {
                    var firstError = validationResult.Errors.First();
                    return firstError.ErrorCode switch
                    {
                        "Unauthorized" => Results.Unauthorized(),
                        _ => Results.BadRequest(new { error = firstError.ErrorMessage, errorCode = firstError.ErrorCode })
                    };
                }

                var (token, expiresAt) = tokenService.GenerateToken(user!);

                httpContext.Response.Cookies.Append(AuthCookieName, token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = httpContext.Request.IsHttps,
                    SameSite = SameSiteMode.Strict,
                    Expires = expiresAt
                });

                var response = new LoginResponse(user!.Username, expiresAt);

                return Results.Ok(response);
            });
        }

    }
}
