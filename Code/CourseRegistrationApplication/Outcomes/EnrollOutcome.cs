namespace CourseRegistrationApplication.Outcomes;

public abstract record EnrollOutcome
{
    public sealed record Enrolled : EnrollOutcome;

    public sealed record Waitlisted(int Position) : EnrollOutcome;

    public sealed record AlreadyEnrolled : EnrollOutcome;

    public sealed record AlreadyWaitlisted(int Position) : EnrollOutcome;

    public sealed record RejectedPrerequisiteNotMet : EnrollOutcome;

    public sealed record RejectedUnknownLearner : EnrollOutcome;
}
