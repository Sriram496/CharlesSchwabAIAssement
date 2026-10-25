using CourseRegistrationApplication.Outcomes;
using CourseRegistrationApplication.Services;

namespace CourseRegistrationApplication.Controllers;

public sealed class EnrollmentController
{
    private const string UnknownCommandMessage = "Unknown command. Use: enroll <learnerId>, drop <learnerId>, status <learnerId>, quit.";

    private static readonly char[] Separators = [' ', '\t'];

    private readonly ICourseOfferingService _service;

    public EnrollmentController(ICourseOfferingService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
    }

    public string Handle(string commandLine)
    {
        string[] parts = commandLine.Trim().Split(Separators, 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return UnknownCommandMessage;
        }

        string learnerId = parts.Length > 1 ? parts[1].Trim() : string.Empty;

        return parts[0].ToLowerInvariant() switch
        {
            "enroll" => FormatEnroll(learnerId),
            "drop" => FormatDrop(learnerId),
            "status" => FormatStatus(learnerId),
            _ => UnknownCommandMessage,
        };
    }

    private string FormatEnroll(string learnerId)
    {
        EnrollOutcome outcome = string.IsNullOrWhiteSpace(learnerId)
            ? new EnrollOutcome.RejectedUnknownLearner()
            : _service.Enroll(learnerId);

        return outcome switch
        {
            EnrollOutcome.Enrolled => $"{learnerId} is enrolled.",
            EnrollOutcome.Waitlisted w => $"{learnerId} is on the waiting list at position {w.Position}.",
            EnrollOutcome.AlreadyEnrolled => $"{learnerId} is already enrolled. No change.",
            EnrollOutcome.AlreadyWaitlisted w => $"{learnerId} is already on the waiting list at position {w.Position}. No change.",
            EnrollOutcome.RejectedPrerequisiteNotMet => $"Rejected: {learnerId} has not met the prerequisite.",
            EnrollOutcome.RejectedUnknownLearner => $"Rejected: unknown learner '{learnerId}'.",
            _ => throw new InvalidOperationException("Unhandled enroll outcome."),
        };
    }

    private string FormatDrop(string learnerId)
    {
        DropOutcome outcome = string.IsNullOrWhiteSpace(learnerId)
            ? new DropOutcome.RejectedUnknownLearner()
            : _service.Drop(learnerId);

        return outcome switch
        {
            DropOutcome.Dropped d when d.PromotedLearnerId is null => $"{learnerId} is dropped. No learner was promoted.",
            DropOutcome.Dropped d => $"{learnerId} is dropped. {d.PromotedLearnerId} was promoted from the waiting list.",
            DropOutcome.RemovedFromWaitlist => $"{learnerId} is removed from the waiting list.",
            DropOutcome.RejectedNotEnrolled => $"Rejected: {learnerId} is not enrolled or on the waiting list.",
            DropOutcome.RejectedUnknownLearner => $"Rejected: unknown learner '{learnerId}'.",
            _ => throw new InvalidOperationException("Unhandled drop outcome."),
        };
    }

    private string FormatStatus(string learnerId)
    {
        StatusOutcome outcome = string.IsNullOrWhiteSpace(learnerId)
            ? new StatusOutcome.RejectedUnknownLearner()
            : _service.GetStatus(learnerId);

        return outcome switch
        {
            StatusOutcome.Enrolled => $"{learnerId} is enrolled.",
            StatusOutcome.Waitlisted w => $"{learnerId} is on the waiting list at position {w.Position}.",
            StatusOutcome.NotEnrolledOrWaitlisted => $"{learnerId} is not enrolled or on the waiting list.",
            StatusOutcome.RejectedUnknownLearner => $"Rejected: unknown learner '{learnerId}'.",
            _ => throw new InvalidOperationException("Unhandled status outcome."),
        };
    }
}
