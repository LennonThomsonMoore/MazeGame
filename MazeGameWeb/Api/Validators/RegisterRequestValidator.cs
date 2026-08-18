using FluentValidation;
using FluentValidation.Results;
using MazeGame.Api.Contracts;
using MazeGame.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace MazeGame.Api.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator(UserDbContext dbContext)
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters long.")
                .MaximumLength(32).WithMessage("Username must be at most 32 characters long.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");

            RuleFor(x => x).CustomAsync(async (request, context, cancellationToken) =>
            {
                if (string.IsNullOrEmpty(request.Username))
                {
                    return;
                }

                var userExists = await dbContext.Users
                    .AnyAsync(u => u.Username == request.Username, cancellationToken);
                if (userExists)
                {
                    context.AddFailure(new ValidationFailure("Username", "User already exists.")
                    {
                        ErrorCode = "Conflict"
                    });
                }
            });
        }
    }
}
