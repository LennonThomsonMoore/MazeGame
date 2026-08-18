using System.Security.Claims;
using FluentValidation;
using FluentValidation.Results;

namespace MazeGame.Api.Validators
{
    public class AuthenticatedUserValidator : AbstractValidator<ClaimsPrincipal>
    {
        public AuthenticatedUserValidator()
        {
            RuleFor(x => x).CustomAsync(async (user, context, cancellationToken) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out _))
                {
                    context.AddFailure(new ValidationFailure("UserId", "You must be logged in to perform this action.")
                    {
                        ErrorCode = "Unauthorized"
                    });
                }
                await Task.CompletedTask;
            });
        }
    }
}
