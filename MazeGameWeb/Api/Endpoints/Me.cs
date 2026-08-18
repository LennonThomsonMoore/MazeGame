using System.Security.Claims;
using MazeGame.Api.Contracts;
using MazeGame.Api.Validators;
using FluentValidation;

namespace MazeGame.Api.Endpoints
{
    public static class Me
    {
        public static void MapMeEndpoint(this WebApplication app)
        {
            app.MapGet("/auth/me", async (HttpContext httpContext, IValidator<ClaimsPrincipal> validator) =>
            {
                var validationResult = await validator.ValidateAsync(httpContext.User);
                if (!validationResult.IsValid)
                {
                    var firstError = validationResult.Errors.First();
                    return firstError.ErrorCode switch
                    {
                        "Unauthorized" => Results.Unauthorized(),
                        _ => Results.BadRequest(new { error = firstError.ErrorMessage, errorCode = firstError.ErrorCode })
                    };
                }

                var username = httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(username))
                {
                    return Results.Unauthorized();
                }

                return Results.Ok(new MeResponse(username));
            }).RequireAuthorization();
        }
    }
}
