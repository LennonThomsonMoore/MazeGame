using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;

namespace MazeGame.Api.Validators
{
    public class LoginRequestValidator : AbstractValidator<UserWithLoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.loginRequest.Username)
                .NotEmpty().WithMessage("Username is required.");

            RuleFor(x => x.loginRequest.Password)
                .NotEmpty().WithMessage("Password is required.");

            RuleFor(x => x).CustomAsync(async (userWithRequest, context, cancellationToken) =>
            {
                var user = userWithRequest.user;
                var request = userWithRequest.loginRequest;
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return;
                }

                if (user == null)
                {
                    context.AddFailure(new ValidationFailure("Username", "Invalid username or password.")
                    {
                        ErrorCode = "Unauthorized"
                    });
                    return;
                }

                var passwordHasher = new PasswordHasher<string>();
                if (passwordHasher.VerifyHashedPassword(user.Username, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
                {
                    context.AddFailure(new ValidationFailure("Password", "Invalid username or password.")
                    {
                        ErrorCode = "Unauthorized"
                    });
                }
            });
        }
    }
}
