using FluentValidation;

namespace ERMS.Application.Features.Auth;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(request => request.CurrentPassword)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(request => request.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128);

        RuleFor(request => request.ConfirmPassword)
            .NotEmpty()
            .Equal(request => request.NewPassword)
            .WithMessage("Confirm password must match the new password.");
    }
}
