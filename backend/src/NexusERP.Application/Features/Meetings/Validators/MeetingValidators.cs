using FluentValidation;
using NexusERP.Application.DTOs.Meetings;

namespace NexusERP.Application.Features.Meetings.Validators;

public class CreateMeetingRequestValidator : AbstractValidator<CreateMeetingRequest>
{
    public CreateMeetingRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EndAt).GreaterThan(x => x.StartAt).WithMessage("End time must be after start time.");
        RuleFor(x => x.Location).MaximumLength(500);
    }
}

public class UpdateMeetingRequestValidator : AbstractValidator<UpdateMeetingRequest>
{
    public UpdateMeetingRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EndAt).GreaterThan(x => x.StartAt).WithMessage("End time must be after start time.");
        RuleFor(x => x.Location).MaximumLength(500);
    }
}
