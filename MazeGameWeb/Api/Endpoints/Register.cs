using MazeGame.Api.Data;
using MazeGame.Api.Models;
using MazeGame.Api.Contracts;
using MazeGame.Api.Validators;
using MazeGame.Api.Services;
using FluentValidation;

namespace MazeGame.Api.Endpoints
{
    public static class Register
    {
        public static void MapRegisterEndpoint(this WebApplication app)
        {
            app.MapPost("/auth/register", async (HttpContext httpContext, UserDbContext dbContext, RegisterRequest request, IValidator<RegisterRequest> validator, ITokenService tokenService) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    var firstError = validationResult.Errors.First();
                    return firstError.ErrorCode switch
                    {
                        "Conflict" => Results.Conflict(new { error = firstError.ErrorMessage, errorCode = firstError.ErrorCode }),
                        _ => Results.BadRequest(new { error = firstError.ErrorMessage, errorCode = firstError.ErrorCode })
                    };
                }

                var user = User.CreateUser(request.Username, request.Password);
                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();

                var (token, expiresAt) = tokenService.GenerateToken(user);

                httpContext.Response.Cookies.Append(Login.AuthCookieName, token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = httpContext.Request.IsHttps,
                    SameSite = SameSiteMode.Strict,
                    Expires = expiresAt
                });

                var response = new RegisterResponse
                (
                    user.Id,
                    user.Username,
                    user.CreatedAt
                );
                return Results.Ok(response);
            });
        }
    }
}
