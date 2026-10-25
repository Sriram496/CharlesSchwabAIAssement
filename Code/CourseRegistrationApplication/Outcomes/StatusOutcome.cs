namespace CourseRegistrationApplication.Outcomes;

public abstract record StatusOutcome
{
    public sealed record Enrolled : StatusOutcome;

    public sealed record Waitlisted(int Position) : StatusOutcome;

    public sealed record NotEnrolledOrWaitlisted : StatusOutcome;

    public sealed record RejectedUnknownLearner : StatusOutcome;
}
