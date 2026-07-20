using FluentValidation;

namespace ERMS.Application.Features.Auth.Validators;

public class ForceChangePasswordRequestValidator : AbstractValidator<ForceChangePasswordRequest>
{
    public ForceChangePasswordRequestValidator()
    {
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
