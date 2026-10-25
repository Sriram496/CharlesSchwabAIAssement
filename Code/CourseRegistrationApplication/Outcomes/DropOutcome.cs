namespace CourseRegistrationApplication.Outcomes;

public abstract record DropOutcome
{
    public sealed record Dropped(string? PromotedLearnerId) : DropOutcome;

    public sealed record RemovedFromWaitlist : DropOutcome;

    public sealed record RejectedNotEnrolled : DropOutcome;

    public sealed record RejectedUnknownLearner : DropOutcome;
}
